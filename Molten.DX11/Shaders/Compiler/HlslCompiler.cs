using Silk.NET.Direct3D.Compilers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    // TODO: implement DXC compiler: https://simoncoenen.com/blog/programming/graphics/DxcCompiling

    internal unsafe class HlslCompiler
    {
        Dictionary<HlslCompilerFlags, string> _argLookup = new Dictionary<HlslCompilerFlags, string>()
        {
            [HlslCompilerFlags.AllResourcesBound] = DXC.ArgAllResourcesBound,
            [HlslCompilerFlags.AvoidFlowControl] = DXC.ArgAvoidFlowControl,
            [HlslCompilerFlags.Debug] = DXC.ArgDebug,
            [HlslCompilerFlags.DebugNameForBinary] = DXC.ArgDebugNameForBinary,
            [HlslCompilerFlags.DebugNameForSource] = DXC.ArgDebugNameForSource,
            [HlslCompilerFlags.EnableBackwardsCompatibility] = DXC.ArgEnableBackwardsCompatibility,
            [HlslCompilerFlags.EnableStrictness] = DXC.ArgEnableStrictness,
            [HlslCompilerFlags.IeeeStrictness] = DXC.ArgIeeeStrictness,
            [HlslCompilerFlags.None] = "",
            [HlslCompilerFlags.OptimizationLevel0] = DXC.ArgOptimizationLevel0,
            [HlslCompilerFlags.OptimizationLevel1] = DXC.ArgOptimizationLevel1,
            [HlslCompilerFlags.OptimizationLevel2] = DXC.ArgOptimizationLevel2,
            [HlslCompilerFlags.OptimizationLevel3] = DXC.ArgOptimizationLevel3,
            [HlslCompilerFlags.PackMatrixColumnMajor] = DXC.ArgPackMatrixColumnMajor,
            [HlslCompilerFlags.PackMatrixRowMajor] = DXC.ArgPackMatrixRowMajor,
            [HlslCompilerFlags.PreferFlowControl] = DXC.ArgPreferFlowControl,
            [HlslCompilerFlags.ResourcesMayAlias] = DXC.ArgResourcesMayAlias,
            [HlslCompilerFlags.SkipOptimizations] = DXC.ArgSkipOptimizations,
            [HlslCompilerFlags.SkipValidation] = DXC.ArgSkipValidation,
            [HlslCompilerFlags.WarningsAreErrors] = DXC.ArgWarningsAreErrors,
        };

        internal static readonly string[] NewLineSeparators = new string[] { "\n", Environment.NewLine };
        Dictionary<string, HlslSubCompiler> _subCompilers;
        Logger _log;
        RendererDX11 _renderer;
        HlslIncluder _defaultIncluder;
        Dictionary<string, ShaderNodeParser> _parsers;

        internal HlslCompiler(RendererDX11 renderer, Logger log)
        {
            // Detect and instantiate node parsers
            _parsers = new Dictionary<string, ShaderNodeParser>();
            IEnumerable<Type> parserTypes = ReflectionHelper.FindTypeInParentAssembly<ShaderNodeParser>();
            foreach (Type t in parserTypes)
            {
                ShaderNodeParser parser = Activator.CreateInstance(t, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null) as ShaderNodeParser;
                foreach (string nodeName in parser.SupportedNodes)
                    _parsers.Add(nodeName, parser);
            }

            Dxc = DXC.GetApi();
            IDxcUtils* dxcUtil = null;
            Dxc.CreateInstance(rclsid, IDxcUtils.Guid, ref dxcUtil); // TODO should rclsid be IDcUtils.Guid? or should riid?
            Utils = dxcUtil;

            _renderer = renderer;
            _log = log;
            _subCompilers = new Dictionary<string, HlslSubCompiler>();
            _defaultIncluder = new EmbeddedIncluder(this, typeof(EmbeddedIncluder).Assembly);

            AddSubCompiler<MaterialCompiler>("material");
            AddSubCompiler<ComputeCompiler>("compute");
        }

        /*
         *  EXAMPLE:
         *  
         *  <material>
         *      <rasterizer>
         *          <cull>clockwise</cull>
         *          <depthBias>0</depthBias>
         *          <depthClamp>1.0</depthClamp>
         *          <fill>solid</fill>
         *          <aaLine>false</aaLine>                                          // IsAntialiasedLineEnabled
         *          <depthClip>false</depthClip>                                    // IsDepthClipEnabled
         *          <front>clockwise</front>                                        // IsFrontCounterClockwise
         *          <multisample>false</multisample>                                // IsMultisampleEnabled
         *          <scissor>0,0,512,512</scissor>                                  // IsScissorEnabled + Scissor rectangle -- Do we allow control over this?
         *          <slopeBias>0</slopeBias>                                        // SlopeScaledDepthBias
         *      </rasterizer>
         *      <blend>
         *          <enabled>true</enabled>                                         // IsBlendEnabled
         *              OR
         *          <enabled rt="1">false</enabled>                                 // IsBlendEnabled for RT index 1. If the rt attribute is invalid or not present, treat it as rt index 0.
         *          <SourceBlend rt="1">InverseDestinationAlpha</SourceBlend>       // InverseDestinationAlpha
         *          // ... etc
         *      </blend>
         *      <depth>
         *          <comparison>LessEqual</comparison>
         *          // ... etc
         *      </depth>
         *      <name>G-Buffer Material</name>
         *      <description>A description of the material</description>
         *      <pass>
         *          <name>blur</name>
         *          <iterations>2</interations>
         *          <vertex>VS_Main</vertex>
         *          <geometry>GS_Main</geometry>
         *          <hull>H_Main</hull>
         *          <domain>Light_Domain</doman>
         *          <pixel>PS_Main</pixel>
         *          <rasterizer>
         *              // [Optional] Pass-specific rasterizer state. Placing inside a pass overrides any states defined in the material root. See above for example.
         *          </rasterizer
         *          <blend>
         *              // [Optional] Pass-specific blend state.
         *          </blend
         *          <depth>
         *              // [Optional] Pass-specific depth state.
         *          </depth
         *      <pass>
         *  </material>
         *  
         *  <compute>
         *      <name>G-Buffer Material</name>
         *      <description>A description of the compute task</description>
         *      <entry>CS_Main</entry>
         *  </compute>
         * 
         *  HLSL code from here and onwards.
         */

        private void AddSubCompiler<T>(string nodeName) where T : HlslSubCompiler
        {
            Type t = typeof(T);
            HlslSubCompiler sub = Activator.CreateInstance(t, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,  null, null, null) as HlslSubCompiler;
            _subCompilers.Add(nodeName, sub);
        }

        internal ShaderCompileResult CompileEmbedded(string filename)
        {
            string source = null;
            using (Stream stream = EmbeddedResource.GetStream(filename, typeof(RendererDX11).Assembly))
            {
                using (StreamReader reader = new StreamReader(stream))
                    source = reader.ReadToEnd();
            }

            return Compile(source, filename);
        }

        internal ShaderCompileResult Compile(string source, string filename = null, HlslIncluder includer = null)
        {
            ShaderCompilerContext context = new ShaderCompilerContext() { Compiler = this };
            Dictionary<string, List<string>> headers = new Dictionary<string, List<string>>();
            string finalSource = source;

            foreach (string nodeName in _subCompilers.Keys)
            {
                List<string> nodeHeaders = GetHeaders(nodeName, source);
                if (nodeHeaders.Count > 0)
                {
                    headers.Add(nodeName, nodeHeaders);

                    // Remove the XML Molten headers from the source.
                    // This reduces the source we need to check through to find other header types.
                    foreach (string h in nodeHeaders)
                    {
                        int index = source.IndexOf(h);
                        string[] lines = source.Substring(0, index).Split(NewLineSeparators, StringSplitOptions.None);
                        string[] hLines = h.Split(NewLineSeparators, StringSplitOptions.None);
                        int endLine = lines.Length + (hLines.Length - 1);

                        if (filename == null)
                            finalSource = finalSource.Replace(h, $"#line {endLine}");
                        else
                            finalSource = finalSource.Replace(h, $"#line {endLine} \"{filename}\"");
                    }
                }
            }

            context.Source = finalSource;
            context.Filename = filename;

            // Compile any headers that matching _subCompiler keys (e.g. material or compute)
            foreach (string nodeName in headers.Keys)
            {
                HlslSubCompiler com = _subCompilers[nodeName];
                List<string> nodeHeaders = headers[nodeName];
                foreach (string header in nodeHeaders)
                {
                    List<IShader> parseResult = com.Parse(context, _renderer, header);

                    // Intialize the shader's default resource array, now that we have the final count of the shader's actual resources.
                    foreach (HlslShader shader in parseResult)
                        shader.DefaultResources = new IShaderResource[shader.Resources.Length];

                    context.Result.AddResult(nodeName, parseResult);
                }
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                foreach (string error in context.Errors)
                    _log.WriteError(error);

                foreach (string msg in context.Messages)
                    _log.WriteLine(msg);
            }
            else
            {
                foreach (string error in context.Errors)
                    _log.WriteError($"{filename}: {error}");

                foreach (string msg in context.Messages)
                    _log.WriteLine($"{filename}: {msg}");
            }

            return context.Result;
        }

        private List<string> GetHeaders(string headerTagName, string source)
        {
            List<string> headers = new List<string>();

            Match m = Regex.Match(source, $"<{headerTagName}>(.|\n)*?</{headerTagName}>");
            while (m.Success)
            {
                headers.Add(m.Value);
                m = m.NextMatch();
            }

            return headers;
        }

        internal void ParserHeader(HlslFoundation foundation, ref string header, ShaderCompilerContext context)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(header);

            XmlNode rootNode = doc.ChildNodes[0];
            ParseNode(foundation, rootNode, context);
        }

        internal void ParseNode(HlslFoundation foundation, XmlNode parentNode, ShaderCompilerContext context)
        {
            foreach (XmlNode node in parentNode.ChildNodes)
            {
                string nodeName = node.Name.ToLower();
                ShaderNodeParser parser = null;
                if (_parsers.TryGetValue(nodeName, out parser))
                {
                    parser.Parse(foundation, context, node);
                }
                else
                {
                    if (parentNode.ParentNode != null)
                        context.Messages.Add($"Ignoring unsupported {parentNode.ParentNode.Name} tag '{parentNode.Name}'");
                    else
                        context.Messages.Add($"Ignoring unsupported root tag '{parentNode.Name}'");
                }
            }
        }

        internal IDxcUtils* Utils { get; }

        internal DXC Dxc { get; }

        internal DeviceDX11 Device => _renderer.Device;
    }
}
