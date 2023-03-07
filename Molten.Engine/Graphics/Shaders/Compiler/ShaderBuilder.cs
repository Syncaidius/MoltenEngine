using System.Text.RegularExpressions;

namespace Molten.Graphics
{
    internal unsafe class ShaderBuilder 
    {
        ShaderLayoutValidator _layoutValidator = new ShaderLayoutValidator();
        ShaderType[] _mandatoryShaders = { ShaderType.Vertex, ShaderType.Pixel }; 
        Regex _regexHeader;

        internal ShaderBuilder()
        {
            _regexHeader = new Regex($"<shader>(.|\n)*?</shader>");
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

        public List<HlslShader> Build(ShaderCompilerContext context,
            RenderService renderer, string header)
        {
            List<HlslShader> result = new List<HlslShader>();
            HlslShader shader = new HlslShader(renderer.Device, context.Source.Filename);

            try
            {
                context.Compiler.ParserHeader(shader, header, context);

                if (shader.Passes == null || shader.Passes.Length == 0)
                {
                    context.AddError($"Material '{shader.Name}' is invalid: No passes defined");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(shader.Passes[0][ShaderType.Vertex].EntryPoint))
                    {
                        context.AddError($"Material '{shader.Name} is invalid: First pass must define a vertex shader (VS) entry point");
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                context.AddError($"{context.Source.Filename ?? "Material header error"}: {e.Message}");
                renderer.Device.Log.Error(e);
                return result;
            }

            // Proceed to compiling each material pass.
            PassCompileResult firstPassResult = null;
            foreach (HlslPass pass in shader.Passes)
            {
                PassCompileResult passResult = CompilePass(context, pass);
                firstPassResult = firstPassResult ?? passResult;

                if (context.HasErrors)
                    return result;
            }

            // No issues arose, lets add it to the result
            if (!context.HasErrors)
            {
                foreach (HlslPass pass in shader.Passes)
                {
                    if (!pass.IsInitialized)
                        pass.Initialize(GraphicsStatePreset.Default, PrimitiveTopology.Triangle);


                    for (int i = 0; i < pass.Samplers.Length; i++)
                        pass.Samplers[i] = pass.Samplers[i] ?? pass.Device.DefaultSampler;
                }

                result.Add(shader);

                shader.Scene = new SceneMaterialProperties(shader);
                shader.Object = new ObjectMaterialProperties(shader);
                shader.Textures = new GBufferTextureProperties(shader);
                shader.SpriteBatch = new SpriteBatchMaterialProperties(shader);
                shader.Light = new LightMaterialProperties(shader);
            }

            // Intialize the shader's default resource array, now that we have the final count of the shader's actual resources.
            foreach (HlslShader r in result)
                r.DefaultResources = new IShaderResource[r.Resources.Length];

            return result;
        }

        private PassCompileResult CompilePass(
            ShaderCompilerContext context,
            HlslPass pass)
        {
            PassCompileResult result = new PassCompileResult(pass);

            // Compile each stage of the material pass.
            foreach(ShaderComposition sc in pass)
            {
                if (string.IsNullOrWhiteSpace(sc.EntryPoint))
                {
                    if (_mandatoryShaders.Contains(sc.Type))
                        context.AddError($"Mandatory entry point for {sc.Type} shader is missing.");

                    continue;
                }

                if (context.Compiler.CompileSource(sc.EntryPoint,
                    sc.Type, context, out ShaderCodeResult cResult))
                {
                    result[sc.Type] = cResult;
                    sc.PtrShader = context.Compiler.BuildShader(pass, sc.Type, cResult.ByteCode);
                    sc.InputStructure = context.Compiler.BuildIO(cResult, sc.Type, ShaderIOStructureType.Input);
                    sc.OutputStructure = context.Compiler.BuildIO(cResult, sc.Type, ShaderIOStructureType.Output);
                }
                else
                {
                    context.AddError($"{context.Source.Filename}: Failed to compile {sc.Type} stage of material pass.");
                    return result;
                }
            }

            if (!context.HasErrors)
            {
                // Fill in any extra metadata
                if (result[ShaderType.Geometry] != null)
                {
                    ShaderCodeResult fcr = result[ShaderType.Geometry];
                    pass.GeometryPrimitive = fcr.Reflection.GSInputPrimitive;
                }

                // Validate I/O structure of each shader stage.
                if (_layoutValidator.Validate(context, result))
                    BuildPassStructure(context, result);
            }

            return result;
        }

        private void BuildPassStructure(
            ShaderCompilerContext context, 
            PassCompileResult pResult)
        {
            HlslPass pass = pResult.Pass;
            HlslShader material = pass.Parent;

            foreach (ShaderType type in pResult.Results.Keys)
            {
                if (pResult[type] == null)
                    continue;

                string typeName = type.ToString().ToLower();
                ShaderComposition comp = pass[type];

                if (comp != null)
                {
                    if (!context.Compiler.BuildStructure(context, material, pResult[type], comp))
                        context.AddError($"Invalid {typeName} shader structure for '{comp.EntryPoint}' in pass '{pResult.Pass.Name}'.");
                }
            }
        }
    }
}
