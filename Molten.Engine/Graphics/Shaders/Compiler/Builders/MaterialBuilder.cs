using Silk.NET.Core.Native;

namespace Molten.Graphics
{
    internal unsafe class MaterialBuilder : ShaderCodeCompiler
    {
        public override ShaderCodeType ClassType => ShaderCodeType.Material;

        MaterialLayoutValidator _layoutValidator = new MaterialLayoutValidator();
        ShaderType[] _mandatoryShaders = { ShaderType.Vertex, ShaderType.Pixel };

        public override List<HlslElement> Build(ShaderCompilerContext context,
            RenderService renderer, in string header)
        {
            List<HlslElement> result = new List<HlslElement>();
            Material material = new Material(renderer.Device, context.Source.Filename);

            try
            {
                context.Compiler.ParserHeader(material, in header, context);

                if (material.Passes == null || material.Passes.Length == 0)
                {
                    context.AddError($"Material '{material.Name}' is invalid: No passes defined");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(material.Passes[0][ShaderType.Vertex].EntryPoint))
                    {
                        context.AddError($"Material '{material.Name} is invalid: First pass must define a vertex shader (VS) entry point");
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
            MaterialPassCompileResult firstPassResult = null;
            foreach (MaterialPass pass in material.Passes)
            {
                MaterialPassCompileResult passResult = CompilePass(context, pass);
                firstPassResult = firstPassResult ?? passResult;

                if (context.HasErrors)
                    return result;
            }

            // No issues arose, lets add it to the result
            if (!context.HasErrors)
            {
                // Set the material's default state. This will be used by passes that are missing a state.
                if (material.DefaultState == null)
                {
                    if (material.PassCount > 0)
                        material.DefaultState = material.Passes[0].State ?? material.Device.DefaultState;
                    else
                        material.DefaultState = material.Device.DefaultState;
                } 

                for (int i = 0; i < material.Samplers.Length; i++)
                    material.Samplers[i] = material.Samplers[i] ?? material.Device.DefaultSampler;

                foreach (MaterialPass pass in material.Passes)
                {
                    pass.State = pass.State ?? material.DefaultState;

                    for (int i = 0; i < pass.Samplers.Length; i++)
                        pass.Samplers[i] = pass.Samplers[i] ?? pass.Device.DefaultSampler;
                }

                result.Add(material);

                material.Scene = new SceneMaterialProperties(material);
                material.Object = new ObjectMaterialProperties(material);
                material.Textures = new GBufferTextureProperties(material);
                material.SpriteBatch = new SpriteBatchMaterialProperties(material);
                material.Light = new LightMaterialProperties(material);
            }

            // Intialize the shader's default resource array, now that we have the final count of the shader's actual resources.
            foreach (HlslShader shader in result)
                shader.DefaultResources = new IShaderResource[shader.Resources.Length];

            return result;
        }

        private MaterialPassCompileResult CompilePass(
            ShaderCompilerContext context,
            MaterialPass pass)
        {
            MaterialPassCompileResult result = new MaterialPassCompileResult(pass);

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
                    sc.InputStructure = context.Compiler.BuildIO(cResult, ShaderIOStructureType.Input);
                    sc.OutputStructure = context.Compiler.BuildIO(cResult, ShaderIOStructureType.Output);
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
            MaterialPassCompileResult pResult)
        {
            MaterialPass pass = pResult.Pass;
            Material material = pass.Material;

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
