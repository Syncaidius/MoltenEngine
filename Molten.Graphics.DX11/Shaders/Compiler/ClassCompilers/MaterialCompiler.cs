using Silk.NET.Core.Native;

namespace Molten.Graphics
{
    internal unsafe class MaterialCompiler : FxcClassCompiler
    {
        public override ShaderClassType ClassType => ShaderClassType.Material;

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
                    if (string.IsNullOrWhiteSpace(material.Passes[0].VS.EntryPoint))
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

            // Validate the vertex input structure of all passes. Should match structure of first pass.
            // Only run this if there is more than 1 pass.
            if (material.PassCount > 1)
            {
                ShaderIOStructure iStructure = material.Passes[0].VS.InputStructure;
                for (int i = 1; i < material.PassCount; i++)
                {
                    if (!material.Passes[i].VS.InputStructure.IsCompatible(iStructure))
                        context.AddError($"Vertex input structure in Pass #{i + 1} in material '{material.Name}' does not match structure of pass #1");
                }
            }

            // No issues arose, lets add it to the material manager
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

                material.InputStructure = material.Passes[0].VS.InputStructure;
                material.InputStructureByteCode = (ID3D10Blob*)firstPassResult[ShaderType.Vertex].ByteCode;
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
            FxcCompiler fxc = context.Compiler as FxcCompiler;

            // Compile each stage of the material pass.
            foreach(ShaderComposition sc in pass.Compositions)
            {
                if (string.IsNullOrWhiteSpace(sc.EntryPoint))
                {
                    if (_mandatoryShaders.Contains(sc.Type))
                        context.AddError($"Mandatory entry point for {sc.Type} shader is missing.");

                    continue;
                }

                if (fxc.CompileSource(sc.EntryPoint,
                    sc.Type, context, out ShaderClassResult cResult))
                {
                    result[sc.Type] = cResult;
                    sc.BuildShader(cResult.ByteCode);
                    sc.InputStructure = BuildIO(cResult, ShaderIOStructureType.Input);
                    sc.OutputStructure = BuildIO(cResult, ShaderIOStructureType.Output);
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
                    ShaderClassResult fcr = result[ShaderType.Geometry];
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

            // Vertex Shader
            if (pResult[ShaderType.Vertex] != null)
            {
                if (!BuildStructure(context, material, pResult[ShaderType.Vertex], pass.VS))
                    context.AddError($"Invalid vertex shader structure for '{pResult.Pass.VS.EntryPoint}' in pass '{pResult.Pass.Name}'.");
            }

            // Hull Shader
            if (pResult[ShaderType.Hull] != null)
            {
                if (!BuildStructure(context, material, pResult[ShaderType.Hull], pass.HS))
                    context.AddError($"Invalid hull shader structure for '{pResult.Pass.HS.EntryPoint}' in pass '{pResult.Pass.Name}'.");
            }

            // Domain Shader
            if (pResult[ShaderType.Domain] != null)
            {
                if (!BuildStructure(context, material, pResult[ShaderType.Domain], pass.DS))
                    context.AddError($"Invalid domain shader structure for '{pResult.Pass.DS.EntryPoint}' in pass '{pResult.Pass.Name}'.");
            }

            // Geometry Shader
            if (pResult[ShaderType.Geometry] != null)
            {
                if (!BuildStructure(context, material, pResult[ShaderType.Geometry], pass.GS))
                    context.AddError($"Invalid geometry shader structure for '{pResult.Pass.GS.EntryPoint}' in pass '{pResult.Pass.Name}'.");
            }

            // Pixel Shader
            if (pResult[ShaderType.Pixel] != null)
            {
                if (!BuildStructure(context, material, pResult[ShaderType.Pixel], pass.PS))
                    context.AddError($"Invalid pixel shader structure for '{pResult.Pass.PS.EntryPoint}' in pass '{pResult.Pass.Name}'.");
            }
        }

        protected override void OnBuildVariableStructure(ShaderCompilerContext context, 
            HlslElement shader, ShaderClassResult result, ShaderResourceInfo info) { }
    }
}
