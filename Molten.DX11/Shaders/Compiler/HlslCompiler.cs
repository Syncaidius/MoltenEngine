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
        MaterialIncludeHandler _includer;

        internal HlslCompiler(RendererDX11 renderer, Logger log)
        {
            _renderer = renderer;
            _log = log;
            _subCompilers = new Dictionary<string, HlslSubCompiler>();
            _includer = new MaterialIncludeHandler();

            AddSubCompiler<MaterialCompiler>("material");
            AddSubCompiler<ComputeCompiler>("compute");
        }

        /* TODO:
         *  - Attach XML-style metadata to the top of a material file containing:
         *      - Entry points
         *      - Shader model version
         *      - Material name
         *      - Material Author
         *      - Description/Instructions
         *  
         *  EXAMPLE:
         *  
         *  <material>
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
         *      <pass>
         *  </material>
         *  
         *  <compute>
         *      <name>G-Buffer Material</name>
         *      <description>A description of the compute task</description>
         *      <entry>CS_Main</entry>
         *  </compute>
         * 
         *  shader code goes here
         */

        private void AddSubCompiler<T>(string nodeName) where T : HlslSubCompiler
        {
            Type t = typeof(T);
            HlslSubCompiler sub = Activator.CreateInstance(t, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,  null, new object[] { _log }, null) as HlslSubCompiler;
            _subCompilers.Add(nodeName, sub);
        }

        internal ShaderParseResult Parse(string source, string filename = null)
        {
            ShaderParseResult result = new ShaderParseResult();
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
                source = ShaderBytecode.Preprocess(source, null, _includer, out hlslError);
            }
            catch (Exception e)
            {
                hlslError = e.Message;
            }

            // Proceed if there is no pre-processor errors.
            if (!string.IsNullOrWhiteSpace(hlslError) == false)
            {
                foreach(string nodeName in headers.Keys)
                {
                    HlslSubCompiler com = _subCompilers[nodeName];
                    List<string> nodeHeaders = headers[nodeName];
                    foreach(string h in nodeHeaders)
                    {
                        ShaderCompileResult parseResult = com.Parse(_renderer, h, source, filename);
                        result.AddResult(nodeName, parseResult);
                    }
                }
            }
            else
            {
                _log.WriteLine($"{filename ?? "Shader source error"}: {hlslError}");
            }

            return result;
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
