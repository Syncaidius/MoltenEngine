using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class HlslCompiler
    {
        Dictionary<string, HlslSubCompiler> _subCompilers;
        Logger _log;
        RendererDX11 _renderer;
        Include _defaultIncluder;

        internal readonly RasterizerNodeParser RasterizerParser;

        internal HlslCompiler(RendererDX11 renderer, Logger log)
        {
            _renderer = renderer;
            _log = log;
            _subCompilers = new Dictionary<string, HlslSubCompiler>();
            _defaultIncluder = new EmbeddedIncludeHandler(typeof(EmbeddedIncludeHandler).Assembly);

            RasterizerParser = new RasterizerNodeParser();

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
         *  HLSL code from and onwards.
         */

        private void AddSubCompiler<T>(string nodeName) where T : HlslSubCompiler
        {
            Type t = typeof(T);
            HlslSubCompiler sub = Activator.CreateInstance(t, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,  null, new object[] { _log }, null) as HlslSubCompiler;
            _subCompilers.Add(nodeName, sub);
        }

        internal ShaderCompileResult Compile(string source, string filename = null, Include includer = null)
        {
            ShaderCompilerContext context = new ShaderCompilerContext(this);
            Dictionary<string, List<string>> headers = new Dictionary<string, List<string>>();

            foreach (string nodeName in _subCompilers.Keys)
            {
                List<string> nodeHeaders = GetHeaders(nodeName, source);
                if (nodeHeaders.Count > 0)
                {
                    headers.Add(nodeName, nodeHeaders);

                    // Remove the headers from the source. This reduces the source we need to check through to find other headear types.
                    foreach (string h in nodeHeaders)
                        source = source.Replace(h, "");
                }
            }

            // Pre-process HLSL source
            string hlslError = "";
            try
            {
                source = ShaderBytecode.Preprocess(source, null, includer ?? _defaultIncluder, out hlslError);
            }
            catch (Exception e)
            {
                hlslError = e.Message;
            }

            // Proceed if there is no pre-processor errors.
            if (!string.IsNullOrWhiteSpace(hlslError) == false)
            {
                context.Source = source;
                context.Filename = filename;

                foreach(string nodeName in headers.Keys)
                {
                    HlslSubCompiler com = _subCompilers[nodeName];
                    List<string> nodeHeaders = headers[nodeName];
                    foreach(string h in nodeHeaders)
                    {
                        context.Header = h;
                        
                        ShaderParseResult parseResult = com.Parse(_renderer, context);
                        context.Result.AddResult(nodeName, parseResult);
                    }
                }
            }
            else
            {
                _log.WriteLine($"{filename ?? "Shader source error"}: {hlslError}");
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
    }
}
