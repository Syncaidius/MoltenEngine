using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace Molten.Graphics
{
    internal unsafe class HlslCompiler : EngineObject
    {
        // For reference or help see the following:
        // See: https://github.com/microsoft/DirectXShaderCompiler/blob/master/include/dxc/dxcapi.h
        // See: https://posts.tanki.ninja/2019/07/11/Using-DXC-In-Practice/
        // See: https://simoncoenen.com/blog/programming/graphics/DxcCompiling

        static readonly Guid CLSID_DxcLibrary= new Guid(0x6245d6af, 0x66e0, 0x48fd, 
            new byte[] {0x80, 0xb4, 0x4d, 0x27, 0x17, 0x96, 0x74, 0x8c});

        static readonly Guid CLSID_DxcUtils = CLSID_DxcLibrary;

        static readonly Guid CLSID_DxcCompilerArgs = new Guid(0x3e56ae82, 0x224d, 0x470f,
            new byte[] { 0xa1, 0xa1, 0xfe, 0x30, 0x16, 0xee, 0x9f, 0x9d });

        static readonly Guid CLSID_DxcCompiler = new Guid(0x73e22d93U, (ushort)0xe6ceU, (ushort)0x47f3U, 
            0xb5, 0xbf, 0xf0, 0x66, 0x4f, 0x39, 0xc1, 0xb0 );

        internal static readonly string[] NewLineSeparators = new string[] { "\n", Environment.NewLine };
        Dictionary<string, HlslSubCompiler> _subCompilers;
        Dictionary<string, HlslSource> _sources;

        Logger _log;
        RendererDX11 _renderer;
        Dictionary<string, ShaderNodeParser> _parsers;
        IDxcCompiler3* _compiler;
        IDxcUtils* _utils;

        internal HlslCompiler(RendererDX11 renderer, Logger log)
        {
            _renderer = renderer;
            _log = log;
            _subCompilers = new Dictionary<string, HlslSubCompiler>();
            _sources = new Dictionary<string, HlslSource>();

            Dxc = DXC.GetApi();
            _utils = CreateDxcInstance<IDxcUtils>(CLSID_DxcUtils, IDxcUtils.Guid);
            _compiler = CreateDxcInstance<IDxcCompiler3>(CLSID_DxcCompiler, IDxcCompiler3.Guid);

            // Detect and instantiate node parsers
            _parsers = new Dictionary<string, ShaderNodeParser>();
            IEnumerable<Type> parserTypes = ReflectionHelper.FindTypeInParentAssembly<ShaderNodeParser>();
            foreach (Type t in parserTypes)
            {
                ShaderNodeParser parser = Activator.CreateInstance(t, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null) as ShaderNodeParser;
                foreach (string nodeName in parser.SupportedNodes)
                    _parsers.Add(nodeName, parser);
            }

            AddSubCompiler<MaterialCompiler>("material");
            AddSubCompiler<ComputeCompiler>("compute");
        }

        protected override void OnDispose()
        {
            SilkUtil.ReleasePtr(ref _compiler);
            SilkUtil.ReleasePtr(ref _utils);
            Dxc.Dispose();
        }

        private T* CreateDxcInstance<T>(Guid clsid, Guid iid) where T : unmanaged
        {
            void* ppv = null;
            HResult result = Dxc.CreateInstance(&clsid, &iid, ref ppv);
            return (T*)ppv;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">The path to the source file. If <paramref name="embedded"/> is true, path should be a namespace.</param>
        /// <returns></returns>
        internal HlslSource LoadSource(string path)
        {
            HlslSource src;

            if (_sources.TryGetValue(path, out src))
                return src;

            string hlslSrc = "";

            try
            {
                using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (StreamReader reader = new StreamReader(stream))
                        hlslSrc = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                _log.WriteError($"An error occurred while reading HLSL source file '{path}': {ex.Message}");
                _log.WriteError(ex, true);
            }

            src = new HlslSource(this, path, ref hlslSrc);
            _sources.Add(path, src);
            return src;
        }

        internal HlslSource LoadEmbeddedSource(string nameSpace, string filename, Assembly assembly)
        {
            string embeddedName = $"{nameSpace}.{filename}";
            HlslSource src;

            if (_sources.TryGetValue(embeddedName, out src))
                return src;

            Stream stream = EmbeddedResource.GetStream(embeddedName, assembly);

            if (stream != null)
            {
                string hlslSrc = "";

                using (StreamReader reader = new StreamReader(stream))
                    hlslSrc = reader.ReadToEnd();

                stream.Dispose();
                src = new HlslSource(this, embeddedName, ref hlslSrc);
                _sources.Add(embeddedName, src);
            }
            else
            {
                _log.WriteError($"Embedded HLSL source file '{embeddedName}' not found");
            }

            return src;
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
            BindingFlags bindFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            HlslSubCompiler sub = Activator.CreateInstance(t, bindFlags,  null, null, null) as HlslSubCompiler;
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

        internal ShaderCompileResult Compile(string source, string filename)
        {
            HlslCompilerContext context = new HlslCompilerContext(this);
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

                        if (string.IsNullOrWhiteSpace(filename))
                            finalSource = finalSource.Replace(h, $"#line {endLine}");
                        else
                            finalSource = finalSource.Replace(h, $"#line {endLine} \"{filename}\"");
                    }
                }
            }


            context.Source = new HlslSource(this, filename, ref source);
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

            string msgPrefix = string.IsNullOrWhiteSpace(filename) ? "" : $"{filename}: ";
            foreach (HlslCompilerContext.Message msg in context.Messages)
                _log.WriteLine($"{msgPrefix}{msg}");

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

        internal void ParserHeader(HlslFoundation foundation, ref string header, HlslCompilerContext context)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(header);

            XmlNode rootNode = doc.ChildNodes[0];
            ParseNode(foundation, rootNode, context);
        }

        internal void ParseNode(HlslFoundation foundation, XmlNode parentNode, HlslCompilerContext context)
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
                        context.AddWarning($"Ignoring unsupported {parentNode.ParentNode.Name} tag '{parentNode.Name}'");
                    else
                        context.AddWarning($"Ignoring unsupported root tag '{parentNode.Name}'");
                }
            }
        }

        internal DXC Dxc { get; }

        internal Device Device => _renderer.Device;

        internal RendererDX11 Renderer => _renderer;

        internal IDxcCompiler3* Native => _compiler;

        internal IDxcUtils* Utils => _utils;
    }
}
