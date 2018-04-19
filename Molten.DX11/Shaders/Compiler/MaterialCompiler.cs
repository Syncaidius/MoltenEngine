using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace Molten.Graphics
{
    internal class MaterialCompiler : HlslSubCompiler
    {
        internal const string MAP_DIFFUSE = "mapDiffuse";
        internal const string MAP_NORMAL = "mapNormal";
        internal const string MAP_EMISSIVE = "mapEmissive";

        // The names for expected constant buffers within each material pass.
        const string CONST_COMMON_NAME = "Common";
        const string CONST_OBJECT_NAME = "Object";
        const string CONST_GBUFFER_NAME = "GBuffer";

        static string[] CONST_COMMON_VAR_NAMES = new string[] { "view", "projection", "viewProjection", "invViewProjection" };
        static string[] CONST_OBJECT_VAR_NAMES = new string[] { "wvp", "world" };
        static string[] CONST_GBUFFER_VAR_NAMES = new string[] { "emissivePower" };

        MaterialLayoutValidator _layoutValidator = new MaterialLayoutValidator();

        internal override List<IShader> Parse(ShaderCompilerContext context, RendererDX11 renderer, string header)
        {
            List<IShader> result = new List<IShader>();
            Material material = new Material(renderer.Device, context.Filename);
            try
            {
                context.Compiler.ParserHeader(material, ref header, context);
                if (material.Passes == null || material.Passes.Length == 0)
                {
                    material.AddDefaultPass();
                    if (string.IsNullOrWhiteSpace(material.Passes[0].VertexShader.EntryPoint))
                    {
                        context.Errors.Add($"Material '{material.Name}' does not have a defined vertex shader entry point. Must be defined in the material or it's first pass.");
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                context.Errors.Add($"{context.Filename ?? "Material header error"}: {e.Message}");
                renderer.Device.Log.WriteError(e);
                return result;
            }

            // Proceed to compiling each material pass.
            MaterialPassCompileResult firstPassResult = null;
            foreach (MaterialPass pass in material.Passes)
            {
                MaterialPassCompileResult passResult = CompilePass(context, pass);
                firstPassResult = firstPassResult ?? passResult;

                context.Messages.AddRange(passResult.Messages);

                if (passResult.Errors.Count > 0)
                {
                    context.Errors.AddRange(passResult.Errors);
                    return result;
                }
            }

            // Populate metadata
            material.Flags |= HasConstantBuffer(context, material, CONST_COMMON_NAME, CONST_COMMON_VAR_NAMES) ? MaterialCommonFlags.Common : MaterialCommonFlags.None;
            material.Flags |= HasConstantBuffer(context, material, CONST_OBJECT_NAME, CONST_OBJECT_VAR_NAMES) ? MaterialCommonFlags.Object : MaterialCommonFlags.None;
            material.Flags |= HasConstantBuffer(context, material, CONST_GBUFFER_NAME, CONST_GBUFFER_VAR_NAMES) ? MaterialCommonFlags.GBuffer : MaterialCommonFlags.None;
            bool hasDiffuse = HasResource(material, MAP_DIFFUSE);
            bool hasNormal = HasResource(material, MAP_NORMAL);
            bool hasEmissive = HasResource(material, MAP_EMISSIVE);
            material.Flags |= hasDiffuse && hasNormal && hasEmissive ? MaterialCommonFlags.GBufferTextures : MaterialCommonFlags.None;

            // Validate the vertex input structure of all passes. Should match structure of first pass.
            // Only run this if there is more than 1 pass.
            if (material.PassCount > 1)
            {
                ShaderIOStructure iStructure = material.Passes[0].VertexShader.InputStructure;
                for (int i = 1; i < material.PassCount; i++)
                {
                    if (!material.Passes[i].VertexShader.InputStructure.IsCompatible(iStructure))
                    {
                        context.Errors.Add($"Vertex input structure in Pass #{i + 1} in material '{material.Name}' does not match structure of pass #1");
                        break;
                    }
                }
            }

            // No issues arose, lets add it to the material manager
            if (context.Errors.Count == 0)
            {
                material.InputStructure = material.Passes[0].VertexShader.InputStructure;
                material.InputStructureByteCode = firstPassResult.VertexResult.Bytecode;
                result.Add(material);
                renderer.Materials.AddMaterial(material);

                if (material.HasFlags(MaterialCommonFlags.Common))
                {
                    material.View = material["view"];
                    material.Projection = material["projection"];
                    material.ViewProjection = material["viewProjection"];
                    material.InvViewProjection = material["invViewProjection"];
                }

                if (material.HasFlags(MaterialCommonFlags.Object))
                {
                    material.World = material["world"];
                    material.Wvp = material["wvp"];
                }

                if (material.HasFlags(MaterialCommonFlags.GBufferTextures))
                {
                    material.DiffuseTexture = material["mapDiffuse"];
                    material.NormalTexture = material["mapNormal"];
                    material.EmissiveTexture = material["mapEmissive"];
                }

                if (material.HasFlags(MaterialCommonFlags.GBuffer))
                {
                    material.EmissivePower = material["emissivePower"];
                }
            }

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

                if (Compile(pass.Compositions[i].EntryPoint, MaterialPass.ShaderTypes[i], context, out result.Results[i]))
                {
                    result.Reflections[i] = BuildIo(result.Results[i], pass.Compositions[i]);
                }
                else
                {
                    result.Errors.Add($"{context.Filename}: Failed to compile {MaterialPass.ShaderTypes[i]} stage of material pass.");
                    return result;
                }
            }

            // Fill in any extra metadata
            if (result.Reflections[MaterialPass.ID_GEOMETRY] != null)
                pass.GeometryPrimitive = result.Reflections[MaterialPass.ID_GEOMETRY].GeometryShaderSInputPrimitive;

            // Validate I/O structure of each shader stage.
            if (_layoutValidator.Validate(result))
                BuildPassStructure(context, result);

            return result;
        }

        private void BuildPassStructure(ShaderCompilerContext context, MaterialPassCompileResult pResult)
        {
            MaterialPass pass = pResult.Pass;
            Material material = pass.Material as Material;
            GraphicsDevice device = material.Device;

            // Vertex Shader
            if (pResult.VertexResult != null)
            {
                if (!BuildStructure(context, material, pResult.VertexReflection, pResult.VertexResult, pass.VertexShader))
                    pResult.Errors.Add($"Invalid vertex shader structure for '{pResult.Pass.VertexShader.EntryPoint}' in pass '{pResult.Pass.Name}'.");
            }

            // Hull Shader
            if (pResult.HullResult != null)
            {
                if (!BuildStructure(context, material, pResult.HullReflection, pResult.HullResult, pass.HullShader))
                    pResult.Errors.Add($"Invalid hull shader structure for '{pResult.Pass.HullShader.EntryPoint}' in pass '{pResult.Pass.Name}'.");
            }

            // Domain Shader
            if (pResult.DomainResult != null)
            {
                if (!BuildStructure(context, material, pResult.DomainReflection, pResult.DomainResult, pass.DomainShader))
                    pResult.Errors.Add($"Invalid domain shader structure for '{pResult.Pass.DomainShader.EntryPoint}' in pass '{pResult.Pass.Name}'.");
            }

            // Geometry Shader
            if (pResult.GeometryResult != null)
            {
                if (!BuildStructure(context, material, pResult.GeometryReflection, pResult.GeometryResult, pass.GeometryShader))
                    pResult.Errors.Add($"Invalid geometry shader structure for '{pResult.Pass.GeometryShader.EntryPoint}' in pass '{pResult.Pass.Name}'.");
            }

            // PixelShader Shader
            if (pResult.PixelResult != null)
            {
                if (!BuildStructure(context, material, pResult.PixelReflection, pResult.PixelResult, pass.PixelShader))
                    pResult.Errors.Add($"Invalid pixel shader structure for '{pResult.Pass.PixelShader.EntryPoint}' in pass '{pResult.Pass.Name}'.");
            }
        }

        protected override void OnBuildVariableStructure(ShaderCompilerContext context, HlslShader shader, ShaderReflection reflection, InputBindingDescription binding, ShaderInputType inputType) { }
    }
}
