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
        ShaderLayoutValidator _layoutValidator = new ShaderLayoutValidator();
        ShaderType[] _mandatoryShaders = { ShaderType.Vertex, ShaderType.Pixel };
        string[] _newLineSeparator = { "\n", Environment.NewLine };
        string[] _includeReplacements = { "#include", "<", ">", "\"" };
        Regex _includeCommas = new Regex("(#include) \"([^\"]*)\"");
        Regex _includeBrackets = new Regex("(#include) <([.+])>");
        Regex _regexHeader = new Regex($"<shader>(.|\n)*?</shader>");

        ConcurrentDictionary<string, ShaderSource> _sources;
        Dictionary<ShaderNodeType, ShaderNodeParser> _nodeParsers;

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
            _sources = new ConcurrentDictionary<string, ShaderSource>();

            InitializeNodeParsers();
        }

        internal List<string> GetHeaders(in string source)
        {
            List<string> headers = new List<string>();
            Match m = _regexHeader.Match(source);

            while (m.Success)
            {
                headers.Add(m.Value);
                m = m.NextMatch();
            }

            return headers;
        }

        private ShaderDefinition ParseDefinition(string xml, ShaderCompilerContext context)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            XmlNode rootNode = doc.ChildNodes[0];
            ShaderDefinition header = new ShaderDefinition();
            ParseNode(header, null, rootNode, context);
            return header;
        }

        private bool ValidateDefinition(ShaderDefinition def, ShaderCompilerContext context)
        {
            if (def.Passes.Count == 0)
            {
                context.AddError($"Shader '{def.Name}' is invalid: No passes defined");
                return false;
            }
            else
            {
                if (def.Passes[0].EntryPoints.TryGetValue(ShaderType.Vertex, out string epVertex))
                {
                    if (string.IsNullOrWhiteSpace(epVertex))
                    {
                        context.AddError($"Shader '{def.Name} is invalid: First pass must define a vertex shader (VS) entry point");
                        return false;
                    }
                }
            }

            return true;
        }

        public HlslShader BuildShader(ShaderCompilerContext context, RenderService renderer, string xmlDef)
        {
            ShaderDefinition def;

            try
            {
                def = ParseDefinition(xmlDef, context);
            }
            catch (Exception e)
            {
                context.AddError($"{context.Source.Filename ?? "Shader definition error"}: {e.Message}");
                renderer.Device.Log.Error(e);
                return null;
            }

            bool valid = ValidateDefinition(def, context);
            if (!valid)
                return null;

            HlslShader shader = new HlslShader(renderer.Device, def, context.Source.Filename);

            // Proceed to compiling each shader pass.
            foreach (ShaderPassDefinition passDef in def.Passes)
            {
                BuildPass(context, shader, def, passDef);
                if (context.HasErrors)
                    return null;
            }

            shader.Scene = new SceneMaterialProperties(shader);
            shader.Object = new ObjectMaterialProperties(shader);
            shader.Textures = new GBufferTextureProperties(shader);
            shader.SpriteBatch = new SpriteBatchMaterialProperties(shader);
            shader.Light = new LightMaterialProperties(shader);

            // Intialize the shader's default resource array, now that we have the final count of the shader's actual resources.
            shader.DefaultResources = new GraphicsResource[shader.Resources.Length];
            return shader;
        }

        private unsafe void BuildPass(ShaderCompilerContext context, HlslShader parent, ShaderDefinition def, ShaderPassDefinition passDef)
        {
            HlslPass pass = Renderer.Device.CreateShaderPass(parent, passDef.Name ?? "Unnamed pass");
            PassCompileResult result = new PassCompileResult(pass);

            // Compile each stage of the material pass.
            foreach (ShaderType epType in passDef.EntryPoints.Keys)
            {
                context.EntryPoint = passDef.EntryPoints[epType];
                context.Type = epType;

                if (string.IsNullOrWhiteSpace(context.EntryPoint))
                {
                    if (_mandatoryShaders.Contains(epType))
                        context.AddError($"Mandatory '{epType}' point for  shader is missing.");

                    continue;
                }

                if (CompileSource(context.EntryPoint, epType, context, out ShaderCodeResult cResult))
                {
                    result[epType] = cResult;
                    ShaderComposition sc = pass.AddComposition(epType);

                    if (Validate(pass, context, cResult))
                    {
                        sc.PtrShader = BuildShader(pass, epType, cResult.ByteCode, cResult.NumBytes);
                        sc.InputStructure = BuildIO(cResult, sc.Type, ShaderIOStructureType.Input);
                        sc.OutputStructure = BuildIO(cResult, sc.Type, ShaderIOStructureType.Output);
                    }
                    else
                    {
                        context.AddError($"{context.Source.Filename}: Validation failed for '{epType}' stage of shader pass.");
                        return;
                    }
                }
                else
                {
                    context.AddError($"{context.Source.Filename}: Failed to compile {epType} stage of shader pass.");
                    return;
                }
            }

            if (!context.HasErrors)
            {
                if (!ValidatePass(pass, ref passDef.Parameters, context))
                    return;

                pass.IsCompute = result[ShaderType.Compute] != null;

                // Fill in any extra metadata
                if (result[ShaderType.Geometry] != null)
                {
                    ShaderCodeResult fcr = result[ShaderType.Geometry];
                    pass.GeometryPrimitive = fcr.Reflection.GSInputPrimitive;
                }


                // Validate I/O structure of each shader stage.
                if (_layoutValidator.Validate(context, result))
                {
                    foreach (ShaderType type in result.Results.Keys)
                    {
                        if (result[type] == null)
                            continue;

                        string typeName = type.ToString().ToLower();
                        ShaderComposition comp = pass[type];

                        if (comp != null)
                        {
                            if (!BuildStructure(context, pass.Parent, result[type], comp))
                                context.AddError($"Invalid {typeName} shader structure for '{comp.EntryPoint}' in pass '{result.Pass.Name}'.");
                        }
                    }

                    // Initialize samplers.
                    pass.Samplers = new ShaderSampler[passDef.Samplers.Length];
                    for (int i = 0; i < passDef.Samplers.Length; i++)
                    {
                        ref ShaderSamplerParameters sp = ref passDef.Samplers[i];
                        pass.Samplers[i] = Renderer.Device.CreateSampler(ref sp);
                    }

                    pass.Initialize(ref passDef.Parameters);
                    parent.AddPass(pass);
                }
            }
        }

        private bool ValidatePass(HlslPass pass, ref ShaderPassParameters parameters, ShaderCompilerContext context)
        {
            if (pass[ShaderType.Hull] != null)
            {
                if (parameters.Topology < PrimitiveTopology.PatchListWith1ControlPoint)
                    context.AddError($"Invalid pass topology '{parameters.Topology}': The pass has a hull shader, so topology must be a patch list");
            }

            if (pass[ShaderType.Compute] != null && pass.CompositionCount > 1)
                context.AddError($"Invalid pass. Pass cannot mix compute entry points with render-stage entry points");

            return !context.HasErrors;
        }

        public abstract ShaderIOStructure BuildIO(ShaderCodeResult result, ShaderType sType, ShaderIOStructureType type);

        public abstract bool CompileSource(string entryPoint, ShaderType type, ShaderCompilerContext context, out ShaderCodeResult result);

        public abstract bool BuildStructure(ShaderCompilerContext context, HlslShader shader, ShaderCodeResult result, ShaderComposition composition);

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

        public ShaderCompileResult Compile(string source, string filename, ShaderCompileFlags flags, Assembly assembly, string nameSpace)
        {
            ShaderCompilerContext context = new ShaderCompilerContext(this);
            context.Flags = flags;
            string finalSource = source;

            if (assembly != null && string.IsNullOrWhiteSpace(nameSpace))
                throw new InvalidOperationException("nameSpace parameter cannot be null or empty when assembly parameter is set");

            int originalLineCount = source.Split(_newLineSeparator, StringSplitOptions.None).Length;

            // Check the source for all supportead class types.
            List<string> nodeHeaders = GetHeaders(in source);

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

            bool isEmbedded = (flags & ShaderCompileFlags.EmbeddedFile) == ShaderCompileFlags.EmbeddedFile;
            context.Source = ParseSource(context, filename, ref finalSource, isEmbedded, assembly, nameSpace, originalLineCount);

            // Compile any headers that matching _subCompiler keys (e.g. material or compute)
            foreach (string header in nodeHeaders)
            {
                HlslShader shader = BuildShader(context, Renderer, header);
                if (shader != null)
                    context.Result.AddShader(shader);
                else
                    context.AddError($"{filename}: {nameof(ShaderCompiler)}.Build() did not return a result (null)");
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

        internal void ParseNode(ShaderDefinition header, ShaderPassDefinition passHeader, XmlNode parentNode, ShaderCompilerContext context)
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

                parser.Parse(header, passHeader, context, node);
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

        protected unsafe abstract void* BuildShader(HlslPass parent, ShaderType type, void* byteCode, nuint numBytes);

        protected abstract bool Validate(HlslPass pass, ShaderCompilerContext context, ShaderCodeResult result);

        public RenderService Renderer { get; }

        /// <summary>
        /// Gets the <see cref="Logger"/> bound to the current <see cref="ShaderCompiler"/> instance.
        /// </summary>
        public Logger Log => Renderer.Log;
    }
}
