using Silk.NET.Core.Native;

namespace Molten.Graphics
{
    internal unsafe class MaterialCompiler : FxcClassCompiler
    {
        public override ShaderClassType ClassType => ShaderClassType.Material;

        MaterialLayoutValidator _layoutValidator = new MaterialLayoutValidator();
        ShaderType[] _mandatoryShaders = { ShaderType.Vertex, ShaderType.Pixel };

        public override List<IShaderElement> Parse(ShaderCompilerContext<RendererDX11, HlslFoundation> context,
            RendererDX11 renderer, in string header)
        {
            List<IShaderElement> result = new List<IShaderElement>();
            Material material = new Material(renderer.NativeDevice, context.Source.Filename);
            try
            {
                context.Compiler.ParserHeader(material, in header, context);
                if (material.Passes == null || material.Passes.Length == 0)
                {
                    material.AddDefaultPass();
                    if (string.IsNullOrWhiteSpace(material.Passes[0].VS.EntryPoint))
                    {
                        context.AddError($"Material '{material.Name}' does not have a defined vertex shader entry point. Must be defined in the material or it's first pass.");
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                context.AddError($"{context.Source.Filename ?? "Material header error"}: {e.Message}");
                renderer.NativeDevice.Log.Error(e);
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
                // Populate missing material states with default.
                material.DepthState.FillMissingWith(renderer.NativeDevice.DepthBank.GetPreset(DepthStencilPreset.Default) as DepthStateDX11);
                material.RasterizerState.FillMissingWith(renderer.NativeDevice.RasterizerBank.GetPreset(RasterizerPreset.Default));
                material.BlendState.FillMissingWith(renderer.NativeDevice.BlendBank.GetPreset(BlendPreset.Default) as BlendStateDX11);

                ShaderSampler defaultSampler = renderer.NativeDevice.SamplerBank.GetPreset(SamplerPreset.Default);
                for (int i = 0; i < material.Samplers.Length; i++)
                    material.Samplers[i].FillMissingWith(defaultSampler);

                // First, attempt to populate pass states with their first conditional state. 
                // If that fails, fill remaining gaps with ones from material.
                foreach (MaterialPass pass in material.Passes)
                {
                    pass.DepthState.FillMissingWith(pass.DepthState[StateConditions.None]);
                    pass.DepthState.FillMissingWith(material.DepthState);

                    pass.RasterizerState.FillMissingWith(pass.RasterizerState[StateConditions.None]);
                    pass.RasterizerState.FillMissingWith(material.RasterizerState);

                    pass.BlendState.FillMissingWith(pass.BlendState[StateConditions.None]);
                    pass.BlendState.FillMissingWith(material.BlendState);

                    // Ensure the pass can at least fit all of the base material samplers (if any).
                    if(pass.Samplers.Length < material.Samplers.Length)
                    {
                        int oldLength = pass.Samplers.Length;
                        Array.Resize(ref pass.Samplers, material.Samplers.Length);
                        for (int i = oldLength; i < pass.Samplers.Length; i++)
                            pass.Samplers[i] = new ShaderStateBank<ShaderSampler>();
                    }

                    for (int i = 0; i < pass.Samplers.Length; i++)
                    {
                        pass.Samplers[i].FillMissingWith(pass.Samplers[i][StateConditions.None]);

                        if (i >= material.Samplers.Length)
                            pass.Samplers[i].FillMissingWith(defaultSampler);
                        else
                            pass.Samplers[i].FillMissingWith(material.Samplers[i]);
                    }
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
            ShaderCompilerContext<RendererDX11, HlslFoundation> context,
            MaterialPass pass)
        {
            MaterialPassCompileResult result = new MaterialPassCompileResult(pass);

            // Compile each stage of the material pass.
            foreach(ShaderComposition sc in pass.Compositions)
            {
                if (string.IsNullOrWhiteSpace(sc.EntryPoint))
                {
                    if (_mandatoryShaders.Contains(sc.Type))
                        context.AddError($"Mandatory entry point for {sc.Type} shader is missing.");

                    continue;
                }

                ShaderClassResult cResult = null;
                if (context.Renderer.ShaderCompiler.CompileSource(sc.EntryPoint,
                    sc.Type, context, out cResult))
                {
                    result[sc.Type] = cResult;
                    sc.SetBytecode((ID3D10Blob*)cResult.ByteCode);
                    sc.InputStructure = BuildIO(cResult, sc, ShaderIOStructureType.Input);
                    sc.OutputStructure = BuildIO(cResult, sc, ShaderIOStructureType.Output);
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
                    pass.GeometryPrimitive = fcr.Reflection.GSInputPrimitive.ToApi();
                }

                // Validate I/O structure of each shader stage.
                if (_layoutValidator.Validate(context, result))
                    BuildPassStructure(context, result);
            }

            return result;
        }

        private void BuildPassStructure(
            ShaderCompilerContext<RendererDX11, HlslFoundation> context, 
            MaterialPassCompileResult pResult)
        {
            MaterialPass pass = pResult.Pass;
            Material material = pass.Material as Material;

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

        protected override void OnBuildVariableStructure(ShaderCompilerContext<RendererDX11, HlslFoundation> context, 
            HlslFoundation shader, ShaderClassResult result, ShaderResourceInfo info) { }
    }
}
