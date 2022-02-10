using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    internal unsafe class MaterialParser : HlslSubCompiler
    {
        MaterialLayoutValidator _layoutValidator = new MaterialLayoutValidator();

        internal override List<IShader> Parse(ShaderCompilerContext context, RendererDX11 renderer, string header)
        {
            List<IShader> result = new List<IShader>();
            Material material = new Material(renderer.Device, context.Source.Filename);
            try
            {
                context.Compiler.ParserHeader(material, ref header, context);
                if (material.Passes == null || material.Passes.Length == 0)
                {
                    material.AddDefaultPass();
                    if (string.IsNullOrWhiteSpace(material.Passes[0].VertexShader.EntryPoint))
                    {
                        context.AddError($"Material '{material.Name}' does not have a defined vertex shader entry point. Must be defined in the material or it's first pass.");
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                context.AddError($"{context.Source.Filename ?? "Material header error"}: {e.Message}");
                renderer.Device.Log.WriteError(e);
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
                ShaderIOStructure iStructure = material.Passes[0].VertexShader.InputStructure;
                for (int i = 1; i < material.PassCount; i++)
                {
                    if (!material.Passes[i].VertexShader.InputStructure.IsCompatible(iStructure))
                        context.AddError($"Vertex input structure in Pass #{i + 1} in material '{material.Name}' does not match structure of pass #1");
                }
            }

            // No issues arose, lets add it to the material manager
            if (context.HasErrors)
            {
                // Populate missing material states with default.
                material.DepthState.FillMissingWith(renderer.Device.DepthBank.GetPreset(DepthStencilPreset.Default));
                material.RasterizerState.FillMissingWith(renderer.Device.RasterizerBank.GetPreset(RasterizerPreset.Default));
                material.BlendState.FillMissingWith(renderer.Device.BlendBank.GetPreset(BlendPreset.Default));

                ShaderSampler defaultSampler = renderer.Device.SamplerBank.GetPreset(SamplerPreset.Default);
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

                material.InputStructure = material.Passes[0].VertexShader.InputStructure;
                material.InputStructureByteCode = firstPassResult.VertexResult.ByteCode;
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

        private MaterialPassCompileResult CompilePass(ShaderCompilerContext context, MaterialPass pass)
        {
            MaterialPassCompileResult result = new MaterialPassCompileResult(pass);

            // Compile each stage of the material pass.
            for(int i = 0; i < MaterialPass.ShaderTypes.Length; i++)
            {
                if (pass.Compositions[i].Optional && string.IsNullOrWhiteSpace(pass.Compositions[i].EntryPoint))
                    continue;

                if (context.Compiler.CompileHlsl(pass.Compositions[i].EntryPoint, 
                    MaterialPass.ShaderTypes[i], context, out result.Results[i]))
                {
                    BuildIO(result.Results[i], pass.Compositions[i]);
                }
                else
                {
                    context.AddError($"{context.Source.Filename}: Failed to compile {MaterialPass.ShaderTypes[i]} stage of material pass.");
                    return result;
                }
            }

            // Fill in any extra metadata
            if (result.GeometryResult != null)
                pass.GeometryPrimitive = result.GeometryResult.Reflection.Ptr->GetGSInputPrimitive();

            // Validate I/O structure of each shader stage.
            if (_layoutValidator.Validate(context, result))
                BuildPassStructure(context, result);

            return result;
        }

        private void BuildPassStructure(ShaderCompilerContext context, MaterialPassCompileResult pResult)
        {
            MaterialPass pass = pResult.Pass;
            Material material = pass.Material as Material;
            Device device = material.Device;

            // Vertex Shader
            if (pResult.VertexResult != null)
            {
                if (!BuildStructure(context, material, pResult.VertexResult, pass.VertexShader))
                    context.AddError($"Invalid vertex shader structure for '{pResult.Pass.VertexShader.EntryPoint}' in pass '{pResult.Pass.Name}'.");
            }

            // Hull Shader
            if (pResult.HullResult != null)
            {
                if (!BuildStructure(context, material, pResult.HullResult, pass.HullShader))
                    context.AddError($"Invalid hull shader structure for '{pResult.Pass.HullShader.EntryPoint}' in pass '{pResult.Pass.Name}'.");
            }

            // Domain Shader
            if (pResult.DomainResult != null)
            {
                if (!BuildStructure(context, material, pResult.DomainResult, pass.DomainShader))
                    context.AddError($"Invalid domain shader structure for '{pResult.Pass.DomainShader.EntryPoint}' in pass '{pResult.Pass.Name}'.");
            }

            // Geometry Shader
            if (pResult.GeometryResult != null)
            {
                if (!BuildStructure(context, material, pResult.GeometryResult, pass.GeometryShader))
                    context.AddError($"Invalid geometry shader structure for '{pResult.Pass.GeometryShader.EntryPoint}' in pass '{pResult.Pass.Name}'.");
            }

            // PixelShader Shader
            if (pResult.PixelResult != null)
            {
                if (!BuildStructure(context, material, pResult.PixelResult, pass.PixelShader))
                    context.AddError($"Invalid pixel shader structure for '{pResult.Pass.PixelShader.EntryPoint}' in pass '{pResult.Pass.Name}'.");
            }
        }

        protected override void OnBuildVariableStructure(ShaderCompilerContext context, HlslShader shader, ShaderCompileResult result, HlslInputBindDescription bind) { }
    }
}
