using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Buffer = Silk.NET.Direct3D.Compilers.Buffer;

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

        string[] _newLineSeparator = { "\n", Environment.NewLine };
        string[] _includeReplacements = { "#include", "<", ">", "\"" };
        static Regex _includeCommas = new Regex("(#include) \"([.+])\"");
        static Regex _includeBrackets = new Regex("(#include) <([.+])>");


        Dictionary<string, HlslParser> _subCompilers;
        Dictionary<string, HlslSource> _sources;

        Logger _log;
        RendererDX11 _renderer;
        IDxcCompiler3* _compiler;
        IDxcUtils* _utils;
        string _defaultIncludePath;
        Assembly _defaultIncludeAssembly;

        /// <summary>
        /// Creates a new instance of <see cref="HlslCompiler"/>.
        /// </summary>
        /// <param name="renderer">The renderer which owns the compiler.</param>
        /// <param name="log"></param>
        /// <param name="includePath">The default path for engine/game HLSL include files.</param>
        /// <param name="includeAssembly"></param>
        internal HlslCompiler(RendererDX11 renderer, Logger log, string includePath, Assembly includeAssembly)
        {
            _defaultIncludePath = includePath;
            _defaultIncludeAssembly = includeAssembly;

            _renderer = renderer;
            _log = log;
            _subCompilers = new Dictionary<string, HlslParser>();
            _sources = new Dictionary<string, HlslSource>();

            Dxc = DXC.GetApi();
            _utils = CreateDxcInstance<IDxcUtils>(CLSID_DxcUtils, IDxcUtils.Guid);
            _compiler = CreateDxcInstance<IDxcCompiler3>(CLSID_DxcCompiler, IDxcCompiler3.Guid);

            // Detect and instantiate node parsers
            Parsers = new Dictionary<string, ShaderNodeParser>();
            IEnumerable<Type> parserTypes = ReflectionHelper.FindTypeInParentAssembly<ShaderNodeParser>();
            foreach (Type t in parserTypes)
            {
                ShaderNodeParser parser = Activator.CreateInstance(t, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null) as ShaderNodeParser;
                foreach (string nodeName in parser.SupportedNodes)
                    Parsers.Add(nodeName, parser);
            }

            AddSubCompiler<MaterialParser>("material");
            AddSubCompiler<ComputeParser>("compute");
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

        private void AddSubCompiler<T>(string nodeName) where T : HlslParser
        {
            Type t = typeof(T);
            BindingFlags bindFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            HlslParser sub = Activator.CreateInstance(t, bindFlags,  null, null, null) as HlslParser;
            _subCompilers.Add(nodeName, sub);
        }

        internal ShaderCompileResult BuildShader(ref string source, string filename, HlslSourceType type, Assembly assembly)
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
                        string[] lines = source.Substring(0, index).Split(_newLineSeparator, StringSplitOptions.None);
                        string[] hLines = h.Split(_newLineSeparator, StringSplitOptions.None);
                        int endLine = lines.Length + (hLines.Length - 1);

                        if (string.IsNullOrWhiteSpace(filename))
                            finalSource = finalSource.Replace(h, $"#line {endLine}");
                        else
                            finalSource = finalSource.Replace(h, $"#line {endLine} \"{filename}\"");
                    }
                }
            }


            context.Source = new HlslSource(filename, ref source, type);

            // See for info: https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-appendix-pre-include
            // Parse #include <file> - Only checks INCLUDE path and "in paths specified by the /I compiler option,
            // in the order in which they are listed."
            // Parse #Include "file" - Above + local source file directory
            ParseSourceDependencies(context, _includeBrackets, false, assembly);
            ParseSourceDependencies(context, _includeCommas, true, assembly); 

            // Compile any headers that matching _subCompiler keys (e.g. material or compute)
            foreach (string nodeName in headers.Keys)
            {
                HlslParser com = _subCompilers[nodeName];
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

        private void ParseSourceDependencies(HlslCompilerContext context, Regex regex, bool allowRelativePath, Assembly assembly)
        {
            HashSet<string> dependencies = new HashSet<string>();
            Match m = regex.Match(context.Source.SourceCode);
            assembly = assembly ?? _defaultIncludeAssembly;

            while (m.Success)
            {
                string depFn = m.Value.ToLower();
                foreach (string rp in _includeReplacements)
                    depFn = depFn.Replace(rp, "");

                depFn = depFn.Trim();

                HlslSource dependency = null;
                string depSource = "";
                Stream fStream = null;
                Assembly depAssembly = null;
                string parsedFilename = null;
                HlslSourceType depType = HlslSourceType.StandardFile;

                if (allowRelativePath)
                {
                    if (context.Source.SourceType == HlslSourceType.StandardFile)
                    {
                        // If the source came from a standard file, check it's local directory for the include.
                        parsedFilename = $"{context.Source.Filename}/{depFn}";
                        if (TryAddDependency(ref parsedFilename, context))
                            continue;

                        if (File.Exists(parsedFilename))
                            fStream = new FileStream(parsedFilename, FileMode.Open, FileAccess.Read);
                    }
                    else if (context.Source.SourceType == HlslSourceType.EmbeddedFile)
                    {
                        // Check parent assembly instead, if set.
                        Assembly parentAssembly = context.Source.ParentAssembly;
                        if (parentAssembly != null)
                        {
                            parsedFilename = $"{depFn},{parentAssembly.FullName}";
                            fStream = EmbeddedResource.TryGetStream(depFn, parentAssembly);
                        }
                    }
                }

                // Check embedded files or the default include path
                if (fStream == null)
                {
                    parsedFilename = $"{depFn},{assembly.FullName}";
                    if (TryAddDependency(ref parsedFilename, context))
                        continue;

                    fStream = EmbeddedResource.TryGetStream(depFn, assembly);

                    if (fStream == null)
                    {
                        parsedFilename = $"{_defaultIncludePath}/{depFn}";
                        if (TryAddDependency(ref parsedFilename, context))
                            continue;

                        if (File.Exists(parsedFilename))
                            fStream = new FileStream(parsedFilename, FileMode.Open, FileAccess.Read);
                    }
                    else
                    {
                        depAssembly = assembly;
                    }
                }

                // Now try to load the dependency
                if (fStream != null)
                {
                    using (StreamReader reader = new StreamReader(fStream))
                        depSource = reader.ReadToEnd();

                    fStream.Dispose();

                    dependency = new HlslSource(parsedFilename, ref depSource, depType, depAssembly);
                    context.Source.Dependencies.Add(dependency);
                    _sources.Add(parsedFilename, dependency);

                    dependencies.Add(parsedFilename);
                }
                else
                {
                    context.AddError($"{context.Source.Filename}: The include '{depFn}' was not found");
                }

                m = m.NextMatch();
            }
        }

        private bool TryAddDependency(ref string path, HlslCompilerContext context)
        {
            if (_sources.TryGetValue(path, out HlslSource dependency))
            {
                context.Source.Dependencies.Add(dependency);
                return true;
            }

            return false;
        }

        /// <summary>Compiles HLSL source code and outputs the result. Returns true if successful, or false if there were errors.</summary>
        /// <param name="log"></param>
        /// <param name="entryPoint"></param>
        /// <param name="type"></param>
        /// <param name="source"></param>
        /// <param name="filename"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        internal bool CompileHlsl(string entryPoint, ShaderType type, HlslCompilerContext context, out HlslCompileResult result)
        {
            // Since it's not possible to have two functions in the same file with the same name, we'll just check if
            // a shader with the same entry-point name is already loaded in the context.
            if (!context.HlslShaders.TryGetValue(entryPoint, out result))
            {
                string strProfile = ShaderModel.Model5_0.ToProfile(type);
                string argString = context.Args.ToString();
                uint argCount = context.Args.Count;
                char** ptrArgString = context.Args.GetArgsPtr();

                Guid dxcResultGuid = IDxcResult.Guid;
                void* dxcResult;
                Buffer srcBuffer = context.Source.BuildFinalSource(context.Compiler);

                Native->Compile(&srcBuffer, ptrArgString, argCount, null, &dxcResultGuid, &dxcResult);
                result = new HlslCompileResult(context, (IDxcResult*)dxcResult);

                SilkMarshal.Free((nint)ptrArgString);

                if (context.HasErrors)
                    return false;

                context.HlslShaders.Add(entryPoint, result);
            }

            return true;
        }

        internal DXC Dxc { get; }

        internal Device Device => _renderer.Device;

        internal RendererDX11 Renderer => _renderer;

        internal IDxcCompiler3* Native => _compiler;

        internal IDxcUtils* Utils => _utils;

        internal Dictionary<string, ShaderNodeParser> Parsers;
    }
}
