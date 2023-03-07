using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Molten.IO;

namespace Molten.Graphics
{
    /// <summary>
    /// Provides a base for implementing a shader compiler.
    /// </summary>
    public abstract class ShaderCompiler : EngineObject
    {
        string[] _newLineSeparator = { "\n", Environment.NewLine };
        string[] _includeReplacements = { "#include", "<", ">", "\"" };
        static Regex _includeCommas = new Regex("(#include) \"([^\"]*)\"");
        static Regex _includeBrackets = new Regex("(#include) <([.+])>");

        ConcurrentDictionary<string, ShaderSource> _sources;
        Dictionary<ShaderNodeType, ShaderNodeParser> _nodeParsers;
        ShaderBuilder _builder;

        Assembly _defaultIncludeAssembly;
        string _defaultIncludePath;

        /// <summary>
        /// Creates a new instance of <see cref="ShaderCompiler"/>.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="includePath"></param>
        /// <param name="includeAssembly"></param>
        protected ShaderCompiler(RenderService renderer, string includePath, Assembly includeAssembly)
        {
            Renderer = renderer;
            _defaultIncludePath = includePath;
            _defaultIncludeAssembly = includeAssembly;

            _nodeParsers = new Dictionary<ShaderNodeType, ShaderNodeParser>();
            _builder = new ShaderBuilder();
            _sources = new ConcurrentDictionary<string, ShaderSource>();

            InitializeNodeParsers();
        }

        protected unsafe abstract ShaderReflection BuildReflection(ShaderCompilerContext context, void* ptrData);

        public abstract ShaderIOStructure BuildIO(ShaderCodeResult result, ShaderType sType, ShaderIOStructureType type);

        public abstract bool CompileSource(string entryPoint, ShaderType type,
            ShaderCompilerContext context, out ShaderCodeResult result);

        public abstract bool BuildStructure(ShaderCompilerContext context,
            HlslShader shader, ShaderCodeResult result, ShaderComposition composition);

        /// <summary>
        /// Registers all <see cref="ShaderNodeParser"/> types in the assembly.
        /// </summary>
        private void InitializeNodeParsers()
        {
            // Find custom and additional node parsers
            Assembly nodeAssembly = GetType().Assembly;
            List<Type> nParserList = ReflectionHelper.FindType<ShaderNodeParser>(nodeAssembly).ToList();

            // Find default/common node parsers.
            IEnumerable<Type> defaultNodeParsers = ReflectionHelper.FindTypeInParentAssembly<ShaderNodeParser>();
            nParserList.AddRange(defaultNodeParsers);

            foreach (Type t in nParserList)
            {
                BindingFlags bFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                ShaderNodeParser nParser = Activator.CreateInstance(t, bFlags, null, null, null) as ShaderNodeParser;

                if (_nodeParsers.ContainsKey(nParser.NodeType))
                {
                    string ntName = nParser.GetType().Name;
                    string existingName = _nodeParsers[nParser.NodeType].GetType().Name;
                    Log.Warning($"Failed to register node parser '{ntName}' for node type '{nParser.NodeType}' as '{existingName}' is already registered.");
                }
                else
                {
                    // Replace existing parser for this node type
                    _nodeParsers[nParser.NodeType] = nParser;
                }
            }

            // Validate parsers to find missing ones.
            ShaderNodeType[] nTypes = Enum.GetValues<ShaderNodeType>();
            foreach (ShaderNodeType t in nTypes)
            {
                if (!_nodeParsers.ContainsKey(t))
                    Log.Error($"Shader compiler '{GetType()}' doesn't provide node parser for '{t}' nodes. May prevent shader compilation.");
            }
        }

        public ShaderCompileResult CompileShader(string source, string filename, ShaderCompileFlags flags, Assembly assembly, string nameSpace)
        {
            ShaderCompilerContext context = new ShaderCompilerContext(this);
            context.Flags = flags;
            string finalSource = source;

            if (assembly != null && string.IsNullOrWhiteSpace(nameSpace))
                throw new InvalidOperationException("nameSpace parameter cannot be null or empty when assembly parameter is set");

            int originalLineCount = source.Split(_newLineSeparator, StringSplitOptions.None).Length;

            // Check the source for all supportead class types.
            List<string> nodeHeaders = _builder.GetHeaders(in source);
            if (nodeHeaders.Count > 0)
            {
                // Remove the XML Molten headers from the source.
                // This reduces the source we need to check through to find other header types.
                foreach (string h in nodeHeaders)
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

            bool isEmbedded = (flags & ShaderCompileFlags.EmbeddedFile) == ShaderCompileFlags.EmbeddedFile;
            context.Source = ParseSource(context, filename, ref finalSource, isEmbedded, assembly, nameSpace, originalLineCount);

            // Compile any headers that matching _subCompiler keys (e.g. material or compute)
            foreach (string header in nodeHeaders)
            {
                List<HlslShader> parseResult = _builder.Build(context, Renderer, header);
                if (parseResult != null)
                    context.Result.AddResult(parseResult);
                else
                    context.AddError($"{filename}: {_builder.GetType().Name}.Parse() did not return a result (null)");
            }

            string msgPrefix = string.IsNullOrWhiteSpace(filename) ? "" : $"{filename}: ";
            foreach (ShaderCompilerMessage msg in context.Messages)
                Log.WriteLine($"{msgPrefix}{msg.Text}");

            return context.Result;
        }

        private ShaderSource ParseSource(ShaderCompilerContext context, string filename, ref string hlsl,
            bool isEmbedded, Assembly assembly, string nameSpace, int originalLineCount)
        {
            ShaderSource source = new ShaderSource(filename, in hlsl, isEmbedded, originalLineCount, assembly, nameSpace);

            // See for info: https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-appendix-pre-include
            // Parse #include <file> - Only checks INCLUDE path and "in paths specified by the /I compiler option,
            // in the order in which they are listed."
            // Parse #Include "file" - Above + local source file directory
            ParseDependencies(context, source, _includeCommas, true, assembly);
            ParseDependencies(context, source, _includeBrackets, false, assembly);

            _sources.TryAdd(source.FullFilename, source);
            return source;
        }

        private ShaderSource TryGetDependency(string path, ShaderSource parent)
        {
            if (_sources.TryGetValue(path, out ShaderSource dependency))
            {
                parent.Dependencies.Add(dependency);
                return dependency;
            }

            return null;
        }

        public void ParserHeader(HlslGraphicsObject shader, string header, ShaderCompilerContext context)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(header);

            XmlNode rootNode = doc.ChildNodes[0];
            ParseNode(shader, rootNode, context);
        }

        public void ParseNode(HlslGraphicsObject shader, XmlNode parentNode, ShaderCompilerContext context)
        {
            foreach (XmlNode node in parentNode.ChildNodes)
            {
                string nodeName = node.Name.ToLower();

                // Skip XML comments.
                if (nodeName == "#comment")
                    continue;

                if (!Enum.TryParse(nodeName, true, out ShaderNodeType nodeType))
                {
                    context.AddWarning($"Node '{nodeName}' is invalid");
                    continue;
                }

                ShaderNodeParser parser = null;
                if (!_nodeParsers.TryGetValue(nodeType, out parser))
                {
                    context.AddWarning($"The node '{nodeName}' is not supported by compiler '{this.GetType().Name}'");
                    continue;
                }

                parser.Parse(shader, context, node);
            }
        }

        private ShaderSource GetDependencySource(ShaderSource source, string key, out Stream stream,
            string embeddedName = null, Assembly parentAssembly = null)
        {
            stream = null;
            ShaderSource dependency;

            if (source.IsEmbedded)
            {
                dependency = TryGetDependency(key, source);
                if (dependency == null)
                {
                    stream = EmbeddedResource.TryGetStream(embeddedName, parentAssembly);
                    return null;
                }
            }
            else
            {
                dependency = TryGetDependency(key, source);
                if (dependency != null)
                {
                    if (File.Exists(key))
                        stream = new FileStream(key, FileMode.Open, FileAccess.Read);

                    return null;
                }
            }

            return dependency;
        }

        private void ParseDependencies(ShaderCompilerContext context, ShaderSource source, Regex regex, bool allowRelativePath, Assembly assembly)
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
                string depKey = null;
                ShaderSource dependency = null;

                if (allowRelativePath)
                {
                    if (source.IsEmbedded)
                    {
                        // Check parent assembly, if set.
                        Assembly parentAssembly = source.ParentAssembly;
                        if (parentAssembly != null)
                        {
                            string embeddedName = $"{source.ParentNamespace}.{depFilename}";
                            depKey = $"{embeddedName}, {parentAssembly.FullName}";
                            dependency = GetDependencySource(source, depKey, out fStream, embeddedName, parentAssembly);
                        }
                    }
                    else
                    {
                        // If the source came from a standard file, check it's local directory for the include.
                        depKey = $"{source.Filename}/{depFilename}";
                        dependency = GetDependencySource(source, depKey, out fStream);
                    }
                }

                // Check embedded files or the default include path
                if (dependency == null && fStream == null)
                {
                    string embeddedName = $"{source.ParentNamespace}.{depFilename}";
                    depKey = $"{source.ParentNamespace}.{depFilename},{assembly.FullName}";
                    dependency = GetDependencySource(source, depKey, out fStream, embeddedName, assembly);
                }

                // Check in default include directory.
                if (dependency == null && fStream == null)
                {
                    depKey = $"{_defaultIncludePath}/{depFilename}";
                    dependency = GetDependencySource(source, depKey, out fStream);
                }

                // Now try to load the dependency, if not found
                if (dependency == null)
                {
                    if (fStream != null)
                    {
                        using (StreamReader reader = new StreamReader(fStream))
                            depSource = reader.ReadToEnd();

                        fStream.Dispose();

                        int depLineCount = depSource.Split(_newLineSeparator, StringSplitOptions.None).Length;
                        dependency = ParseSource(context, depFilename, ref depSource, source.IsEmbedded,
                            assembly, source.ParentNamespace, depLineCount);
                        source.Dependencies.Add(dependency);
                        dependencies.Add(depFilename);
                    }
                    else
                    {

                        context.AddError($"{source.Filename}: The include '{depFilename}' was not found");
                    }
                }

                // Remove the current #include delcaration
                source.SourceCode = source.SourceCode.Replace($"{m.Value};", dependency.SourceCode);
                source.SourceCode = source.SourceCode.Replace(m.Value, dependency.SourceCode);

                m = m.NextMatch();
            }
        }

        public unsafe abstract void* BuildShader(HlslPass parent, ShaderType type, void* byteCode);

        public RenderService Renderer { get; }

        /// <summary>
        /// Gets the <see cref="Logger"/> bound to the current <see cref="ShaderCompiler"/> instance.
        /// </summary>
        public Logger Log => Renderer.Log;
    }

    /*
         *  EXAMPLE material header:
         *  
         *  <shader>
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
         *  </shader>
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
