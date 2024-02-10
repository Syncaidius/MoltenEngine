using Molten.IO;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Molten.Graphics;

/// <summary>
/// Provides a base for implementing a shader compiler.
/// </summary>
public abstract class ShaderCompiler : EngineObject
{
    ShaderLayoutValidator _layoutValidator;
    ShaderStructureBuilder _structureBuilder;
    ShaderType[] _mandatoryShaders = { ShaderType.Vertex, ShaderType.Pixel };
    string[] _newLineSeparator = { "\n", Environment.NewLine };
    string[] _includeReplacements = { "#include", "<", ">", "\"" };
    Regex _includeCommas = new("(#include) \"([^\"]*)\"");
    Regex _includeBrackets = new("(#include) <([.+])>");

    ConcurrentDictionary<string, ShaderSource> _sources;

    Assembly _defaultIncludeAssembly;
    string _defaultIncludePath;

    /// <summary>
    /// Creates a new instance of <see cref="ShaderCompiler"/>.
    /// </summary>
    /// <param name="device"></param>
    /// <param name="includePath"></param>
    /// <param name="includeAssembly"></param>
    protected ShaderCompiler(GraphicsDevice device, string includePath, Assembly includeAssembly)
    {
        Model = ShaderModel.Model5_0;
        Device = device;
        _layoutValidator = new ShaderLayoutValidator();
        _structureBuilder = new ShaderStructureBuilder();
        _defaultIncludePath = includePath;
        _defaultIncludeAssembly = includeAssembly;
        _sources = new ConcurrentDictionary<string, ShaderSource>();
    }

    internal ShaderCompileResult Compile(string jsonDef, string filename, ShaderCompileFlags flags, Assembly assembly, string nameSpace)
    {
        bool isEmbedded = !string.IsNullOrWhiteSpace(nameSpace);
        ShaderCompilerContext context = new ShaderCompilerContext(this);
        context.Flags = flags;

        if (assembly != null && string.IsNullOrWhiteSpace(nameSpace))
            throw new InvalidOperationException("nameSpace parameter cannot be null or empty when assembly parameter is set");

        // Check the source for all supportead class types.
        ShaderDefinition[] definitions = null;
        try
        {
            definitions = JsonConvert.DeserializeObject<ShaderDefinition[]>(jsonDef);

            foreach (ShaderDefinition def in definitions)
            {
                if (!ValidateDefinition(def, context))
                    continue;

                string hlsl = "";

                // Attempt to load the file that was given in the shader definition.
                if (isEmbedded)
                {
                    using (Stream stream = EmbeddedResource.TryGetStream($"{nameSpace}.{def.File}"))
                    {
                        if (stream == null)
                        {
                            context.AddError($"Embedded shader '{nameSpace}.{def.File}' was not found in assembly '{assembly.FullName}'");
                            continue;
                        }

                        using (StreamReader reader = new StreamReader(stream))
                            hlsl = reader.ReadToEnd();
                    }
                }
                else
                {
                    // If the specified def.File is is not an absolute path, use the directory of the definition file as a root.
                    string entryPath = def.File;
                    if (!Path.IsPathFullyQualified(entryPath))
                    {
                        FileInfo mainPath = new FileInfo(filename);
                        entryPath = $"{mainPath.DirectoryName}/{def.File}";
                    }

                    using (FileStream stream = new FileStream(entryPath, FileMode.Open, FileAccess.Read))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                            hlsl = reader.ReadToEnd();
                    }
                }

                context.Source = ParseSource(context, def.File, ref hlsl, isEmbedded, assembly, nameSpace);

                HlslShader shader = BuildShader(context, def);
                if (shader != null)
                    context.Result.AddShader(shader);
                else
                    context.AddError($"{filename}: {nameof(ShaderCompiler)}.Build() did not return a result (null)");
            }
        }
        catch (Exception ex)
        {
            context.AddError($"Failed to deserialize shader definition: {ex.Message}");
        }

        // Log all context messages.
        string msgPrefix;
        if (isEmbedded)
            msgPrefix = $"{nameSpace}.{filename}: ";
        else
            msgPrefix = string.IsNullOrWhiteSpace(filename) ? "" : $"{filename}: ";

        foreach (ShaderCompilerMessage msg in context.Messages)
            Log.WriteLine($"{msgPrefix}{msg.Text}");

        return context.Result;
    }

    private bool ValidateDefinition(ShaderDefinition def, ShaderCompilerContext context)
    {
        if(string.IsNullOrWhiteSpace(def.File))
        {
            context.AddError($"Shader '{def.Name}' is invalid: No entry file defined");
            return false;
        }

        if (def.Passes.Length == 0)
        {
            context.AddError($"Shader '{def.Name}' is invalid: No passes defined");
            return false;
        }

        ShaderPassDefinition firstPass = def.Passes[0];
        if (string.IsNullOrWhiteSpace(firstPass.Entry.Vertex))
        {
            if (string.IsNullOrWhiteSpace(firstPass.Entry.Pixel) 
                && string.IsNullOrWhiteSpace(firstPass.Entry.Geometry) 
                && string.IsNullOrWhiteSpace(firstPass.Entry.Compute))
            {
                context.AddError("Shader '{def.Name}' is invalid: A vertex entry-point is defined, so a geometry, pixel or compute entry-point must also be defined for output.");
                return false;
            }
        }

        return true;
    }

    private HlslShader BuildShader(ShaderCompilerContext context, ShaderDefinition def)
    {
        HlslShader shader = new HlslShader(Device, def, context.Source.Filename);

        // Proceed to compiling each shader pass.
        foreach (ShaderPassDefinition passDef in def.Passes)
        {
            BuildPass(context, shader, passDef);
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

    private unsafe void BuildPass(ShaderCompilerContext context, HlslShader parent, ShaderPassDefinition passDef)
    {
        HlslPass pass = Device.CreateShaderPass(parent, passDef.Name ?? "Unnamed pass");
        PassCompileResult result = new PassCompileResult(pass);

        // Populate the format lookup of the pass parameters.
        foreach (KeyValuePair<string, string> p in passDef.Parameters.RawFormats)
        {
            if (Enum.TryParse(p.Value, true, out GraphicsFormat format))
                passDef.Parameters.Formats[p.Key] = format;
            else if (Enum.TryParse(p.Value, true, out DepthFormat depthFormat))
                passDef.Parameters.Formats[p.Key] = depthFormat.ToGraphicsFormat();
            else
                context.AddError($"Invalid format '{p.Value}' for '{p.Key}' in pass '{passDef.Name}'");
        }

        // Compile each stage of the material pass.
        foreach (ShaderType epType in passDef.Entry.Points.Keys)
        {
            context.EntryPoint = passDef.Entry.Points[epType];
            context.Type = epType;

            if (string.IsNullOrWhiteSpace(context.EntryPoint))
            {
                if (_mandatoryShaders.Contains(epType))
                    context.AddError($"Mandatory {epType} entry-point for shader is missing.");

                continue;
            }

            // Since it's not possible to have two functions in the same file with the same name, we'll just check if
            // a shader with the same entry-point name is already loaded in the context.
            // If not, compile it.
            if (!context.Shaders.TryGetValue(context.EntryPoint, out ShaderCodeResult cResult))
            {
                cResult = CompileNativeSource(context.EntryPoint, epType, context);
                if (cResult != null)
                {
                    if (!Validate(pass, context, cResult))
                    {
                        context.AddError($"{context.Source.Filename}: Validation failed for {epType} stage of shader pass.");
                        return;
                    } 
                    
                    context.Shaders.Add(context.EntryPoint, cResult);
                }
                else
                {
                    context.AddError($"{context.Source.Filename}: Failed to compile {epType} stage of shader pass.");
                    return;
                }
            }

            // At this point, the bytecode has already been validated, so we can proceed.
            result[epType] = cResult;
            ShaderComposition sc = pass.AddComposition(epType);
            sc.PtrShader = BuildNativeShader(pass, epType, cResult.ByteCode, cResult.NumBytes);
            sc.InputLayout = BuildIO(cResult, ShaderIOLayoutType.Input);
            sc.OutputLayout = BuildIO(cResult, ShaderIOLayoutType.Output);

            if(epType == ShaderType.Pixel)
            {
                // Apply output surface formats.
                for(int i = 0; i < sc.OutputLayout.Metadata.Length; i++)
                {
                    uint slot = sc.OutputLayout.Metadata[i].SemanticIndex;
                    if (passDef.Parameters.Formats.TryGetValue($"os{slot}", out GraphicsFormat format))
                        pass.FormatLayout.RawFormats[slot] = (byte)format;
                    else
                        context.AddError($"No format defined for output surface 'os{slot}' in pass '{passDef.Name}'");
                }

                // Apply depth-stencil format.
                if(passDef.Parameters.Formats.TryGetValue("depth", out GraphicsFormat depthFormat))
                    pass.FormatLayout.Depth = depthFormat.ToDepthFormat();
                else
                    context.AddError($"No 'depth' format defined in pass '{passDef.Name}'");
            }
        }

        if (!context.HasErrors)
        {
            if(passDef.Parameters.Blend != BlendPreset.Default)
                passDef.Parameters.ApplyBlendPreset(passDef.Parameters.Blend);

            if (passDef.Parameters.Rasterizer != RasterizerPreset.Default)
                passDef.Parameters.ApplyRasterizerPreset(passDef.Parameters.Rasterizer);

            if (passDef.Parameters.Depth != DepthStencilPreset.Default)
                passDef.Parameters.ApplyDepthPreset(passDef.Parameters.Depth);

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
                        if (!_structureBuilder.Build(context, result[type].Reflection, comp, passDef))
                            context.AddError($"Invalid {typeName} shader structure for '{comp.EntryPoint}' in pass '{result.Pass.Name}'.");
                    }
                }

                // Initialize samplers.
                pass.Samplers = new ShaderSampler[passDef.Samplers.Length];
                for (int i = 0; i < passDef.Samplers.Length; i++)
                {
                    ref ShaderSamplerParameters sp = ref passDef.Samplers[i];
                    sp.ApplyPreset(sp.Preset);
                    pass.Samplers[i] = Device.CreateSampler(ref sp);
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

    private ShaderIOLayout BuildIO(ShaderCodeResult result, ShaderIOLayoutType type)
    {
        ShaderIOLayout layout = Device.LayoutCache.Create();
        layout.Build(result, type);
        Device.LayoutCache.Cache(ref layout);
        return layout;
    }

    private ShaderSource ParseSource(ShaderCompilerContext context, string filename, ref string hlsl,
        bool isEmbedded, Assembly assembly, string nameSpace)
    {
        int originalLineCount = hlsl.Split(_newLineSeparator, StringSplitOptions.None).Length;
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

    private ShaderSource GetDependencySource(ShaderSource source, string key, out Stream stream, string embeddedName = null, Assembly parentAssembly = null)
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
        assembly ??= _defaultIncludeAssembly;

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

                    dependency = ParseSource(context, depFilename, ref depSource, source.IsEmbedded,
                        assembly, source.ParentNamespace);
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

    protected abstract ShaderCodeResult CompileNativeSource(string entryPoint, ShaderType type, ShaderCompilerContext context);

    protected unsafe abstract void* BuildNativeShader(HlslPass parent, ShaderType type, void* byteCode, nuint numBytes);

    protected abstract bool Validate(HlslPass pass, ShaderCompilerContext context, ShaderCodeResult result);

    /// <summary>
    /// Gets the <see cref="Logger"/> bound to the current <see cref="ShaderCompiler"/> instance.
    /// </summary>
    public Logger Log => Device.Log;

    /// <summary>
    /// Gets or sets the <see cref="ShaderModel"/> to use when compiling shaders.
    /// </summary>
    public ShaderModel Model { get; set; }

    /// <summary>
    /// Gets the <see cref="GraphicsDevice"/> that the shader compiler is bound to.
    /// </summary>
    public GraphicsDevice Device { get; }
}
