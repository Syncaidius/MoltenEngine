using Molten.IO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="CXT">Shader compiler context type</typeparam>
    /// <typeparam name="SB">Source build result</typeparam>
    public abstract class ShaderCompiler<R, S, CR> : EngineObject
        where R : RenderService
        where S : IShaderElement
        where CR : ShaderCompileResult<S>
    {
        string[] _newLineSeparator = { "\n", Environment.NewLine };
        string[] _includeReplacements = { "#include", "<", ">", "\"" };
        static Regex _includeCommas = new Regex("(#include) \"([^\"]*)\"");
        static Regex _includeBrackets = new Regex("(#include) <([.+])>");

        public R Renderer { get; }

        public Logger Log { get; }

        ConcurrentDictionary<string, ShaderSource> _sources;
        Dictionary<ShaderNodeType, ShaderNodeParser<R, S, CR>> _nodeParsers;
        List<ShaderClassCompiler<R, S, CR>> _classCompilers;

        Assembly _defaultIncludeAssembly;
        string _defaultIncludePath;

        protected ShaderCompiler(R renderer, string includePath, Assembly includeAssembly)
        {
            Log = renderer.Log;
            _defaultIncludePath = includePath;
            _defaultIncludeAssembly = includeAssembly;

            _nodeParsers = new Dictionary<ShaderNodeType, ShaderNodeParser<R, S, CR>>();
            _classCompilers = new List<ShaderClassCompiler<R, S, CR>>();
            _sources = new ConcurrentDictionary<string, ShaderSource>();
            Renderer = renderer;
        }

        public abstract bool CompileSource(string entryPoint, ShaderType type, 
            ShaderCompilerContext<R,S,CR> context, out CR result);


        /// <summary>
        /// Registers all <see cref="ShaderNodeParser{CXT}"/> types in the assembly of type <typeparamref name="T"/> and inherit
        /// from type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected void RegisterNodeParsers<T>()
            where T : ShaderNodeParser<R, S, CR>
        {
            IEnumerable<Type> parserTypes = ReflectionHelper.FindTypeInParentAssembly<ShaderNodeParser<R, S, CR>>();
            foreach (Type t in parserTypes)
            {
                BindingFlags bFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                ShaderNodeParser<R, S, CR> nParser = 
                    Activator.CreateInstance(t, bFlags, null, null, null) as ShaderNodeParser<R, S, CR>;

                if (_nodeParsers.ContainsKey(nParser.NodeType))
                {
                    string ntName = nParser.GetType().Name;
                    string existingName = _nodeParsers[nParser.NodeType].GetType().Name;
                    Log.WriteError($"Failed to register node parser '{ntName}' for node type '{nParser.NodeType}' as '{existingName}' is already registered.");
                }
                else
                {
                    _nodeParsers[nParser.NodeType] = nParser;
                }
            }
        }

        protected void AddClassCompiler<T>()
            where T : ShaderClassCompiler<R, S, CR>, new()
        {
            Type t = typeof(T);
            BindingFlags bindFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            T scc = Activator.CreateInstance(t, bindFlags, null, null, null) as T;
            _classCompilers.Add(scc);
        }

        public ShaderCompileResult<S> CompileShader(in string source, string filename, ShaderCompileFlags flags, Assembly assembly, string nameSpace)
        {
            ShaderCompilerContext<R, S, CR> context = new ShaderCompilerContext<R, S, CR>(this);
            Dictionary<ShaderClassCompiler<R, S, CR>, List<string>> headers = new Dictionary<ShaderClassCompiler<R, S, CR>, List<string>>();
            string finalSource = source;


            if (assembly != null && string.IsNullOrWhiteSpace(nameSpace))
                throw new InvalidOperationException("nameSpace parameter cannot be null or empty when assembly parameter is set");

            int originalLineCount = source.Split(_newLineSeparator, StringSplitOptions.None).Length;

            // Check the source for all supportead class types.
            foreach (ShaderClassCompiler<R,S,CR> scc in _classCompilers)
            {
                List<string> classHeaders = scc.GetHeaders(in source);
                if (classHeaders.Count > 0)
                {
                    headers.Add(scc, classHeaders);

                    // Remove the XML Molten headers from the source.
                    // This reduces the source we need to check through to find other header types.
                    foreach (string h in classHeaders)
                    {
                        // Find the header in the original source string, so we can get an accurate line number.
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

            bool isEmbedded = (flags & ShaderCompileFlags.EmbeddedFile) == ShaderCompileFlags.EmbeddedFile;
            context.Source = ParseSource(context, filename, ref finalSource, isEmbedded, assembly, nameSpace, originalLineCount);

            // Compile any headers that matching _subCompiler keys (e.g. material or compute)
            foreach (ShaderClassCompiler<R, S, CR> parser in headers.Keys)
            {
                List<string> nodeHeaders = headers[parser];
                foreach (string header in nodeHeaders)
                {
                    List<S> parseResult = parser.Parse(context, Renderer, in header);
                    context.Result.AddResult(parser.ClassType, parseResult);
                }
            }

            string msgPrefix = string.IsNullOrWhiteSpace(filename) ? "" : $"{filename}: ";
            foreach (ShaderCompilerMessage msg in context.Messages)
                Log.WriteLine($"{msgPrefix}{msg.Text}");

            return context.Result;
        }

        private ShaderSource ParseSource(ShaderCompilerContext<R,S,CR> context, string filename, ref string hlsl,
            bool isEmbedded, Assembly assembly, string nameSpace, int originalLineCount)
        {
            ShaderSource source = new ShaderSource(filename, ref hlsl, isEmbedded, originalLineCount, assembly, nameSpace);

            // See for info: https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-appendix-pre-include
            // Parse #include <file> - Only checks INCLUDE path and "in paths specified by the /I compiler option,
            // in the order in which they are listed."
            // Parse #Include "file" - Above + local source file directory
            ParseDependencies(context, source, _includeCommas, true, assembly);
            ParseDependencies(context, source, _includeBrackets, false, assembly);

            _sources.TryAdd(source.FullFilename, source);
            return source;
        }

        private bool TryGetDependency(ref string path, ShaderCompilerContext<R, S, CR> context)
        {
            if (_sources.TryGetValue(path, out ShaderSource dependency))
            {
                context.Source.Dependencies.Add(dependency);
                return true;
            }

            return false;
        }

        public void ParserHeader(S shader, ref string header, ShaderCompilerContext<R, S, CR> context)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(header);

            XmlNode rootNode = doc.ChildNodes[0];
            ParseNode(shader, rootNode, context);
        }

        public void ParseNode(S shader, XmlNode parentNode, ShaderCompilerContext<R, S, CR> context)
        {
            foreach (XmlNode node in parentNode.ChildNodes)
            {
                string nodeName = node.Name.ToLower();
                if (!Enum.TryParse(nodeName, out ShaderNodeType nodeType))
                {
                    context.AddError($"Node '{nodeName}' is invalid");
                    continue;
                }

                ShaderNodeParser<R, S, CR> parser = null;
                if (!_nodeParsers.TryGetValue(nodeType, out parser))
                {
                    context.AddError($"The node '{nodeName}' is not supported by compiler '{this.GetType().Name}'");
                    continue;
                }

                parser.Parse(shader, context, node);
            }
        }

        private void ParseDependencies(ShaderCompilerContext<R, S, CR> context, ShaderSource source, Regex regex, bool allowRelativePath, Assembly assembly)
        {
            HashSet<string> dependencies = new HashSet<string>();
            Match m = regex.Match(source.SourceCode);
            assembly = assembly ?? _defaultIncludeAssembly;

            while (m.Success)
            {
                string depFilename = m.Value.ToLower();
                foreach (string rp in _includeReplacements)
                    depFilename = depFilename.Replace(rp, "");

                depFilename = depFilename.Trim();

                // Don't parse the same include twice. TODO improve this to prevent circular includes with more than 2 files involved.
                if (dependencies.Contains(depFilename))
                    continue;

                string depSource = "";
                Stream fStream = null;
                string parsedFilename = null;
                bool depIsEmbedded = source.IsEmbedded;

                if (allowRelativePath)
                {
                    if (source.IsEmbedded)
                    {
                        // Check parent assembly instead, if set.
                        Assembly parentAssembly = source.ParentAssembly;
                        if (parentAssembly != null)
                        {
                            string embeddedName = $"{source.ParentNamespace}.{depFilename}";
                            parsedFilename = $"{embeddedName}, {parentAssembly.FullName}";
                            if (TryGetDependency(ref parsedFilename, context))
                                continue;

                            fStream = EmbeddedResource.TryGetStream(embeddedName, parentAssembly);
                        }
                    }
                    else
                    {
                        // If the source came from a standard file, check it's local directory for the include.
                        parsedFilename = $"{source.Filename}/{depFilename}";
                        if (TryGetDependency(ref parsedFilename, context))
                            continue;

                        if (File.Exists(parsedFilename))
                            fStream = new FileStream(parsedFilename, FileMode.Open, FileAccess.Read);
                    }
                }

                // Check embedded files or the default include path
                if (fStream == null)
                {
                    parsedFilename = $"{source.ParentNamespace}.{depFilename},{assembly.FullName}";
                    if (TryGetDependency(ref parsedFilename, context))
                        continue;

                    fStream = EmbeddedResource.TryGetStream(depFilename, assembly);

                    // Try default include path instead.
                    if (fStream == null)
                    {
                        parsedFilename = $"{_defaultIncludePath}/{depFilename}";
                        if (TryGetDependency(ref parsedFilename, context))
                            continue;

                        if (File.Exists(parsedFilename))
                            fStream = new FileStream(parsedFilename, FileMode.Open, FileAccess.Read);
                    }
                }

                // Now try to load the dependency
                if (fStream != null)
                {
                    using (StreamReader reader = new StreamReader(fStream))
                        depSource = reader.ReadToEnd();

                    fStream.Dispose();

                    int depLineCount = depSource.Split(_newLineSeparator, StringSplitOptions.None).Length;
                    ShaderSource dependency = ParseSource(context, depFilename, ref depSource, depIsEmbedded,
                        assembly, source.ParentNamespace, depLineCount);
                    source.Dependencies.Add(dependency);
                    dependencies.Add(depFilename);

                    // Remove the current #include delcaration
                    source.SourceCode = source.SourceCode.Replace($"{m.Value};", dependency.SourceCode);
                    source.SourceCode = source.SourceCode.Replace(m.Value, dependency.SourceCode);
                }
                else
                {
                    context.AddError($"{source.Filename}: The include '{depFilename}' was not found");
                }

                m = m.NextMatch();
            }
        }
    }

    /*
         *  EXAMPLE material header:
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
         *  HLSL/GLSL code from here and onwards.
         */
}
