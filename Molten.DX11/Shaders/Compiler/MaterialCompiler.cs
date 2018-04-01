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

        static string[] CONST_COMMON_VAR_NAMES = new string[] { "view", "projection", "viewProjection", "invViewProjection" };
        static string[] CONST_OBJECT_VAR_NAMES = new string[] { "wvp", "world" };

        MaterialLayoutValidator _layoutValidator;

        internal MaterialCompiler(Logger log) : base(log)
        {
            AddParser<ShaderPassParser>("pass");

            _layoutValidator = new MaterialLayoutValidator();
        }

        protected override void ParseHeaderNode(string nodeName, XmlNode node, HlslShader shader, ShaderCompilerContext context)
        {
            Material mat = shader as Material;

            switch (nodeName)
            {
                case "rasterizer":
                    shader.RasterizerState = context.Compiler.RasterizerParser.Parse(shader, context, node);
                    // Run through existing passes and apply the rasterizer state to them if they don't have their own yet
                    foreach (MaterialPass p in mat.Passes)
                        p.RasterizerState = p.RasterizerState ?? shader.RasterizerState;
                    break;

                default:
                    base.ParseHeaderNode(nodeName, node, shader, context);
                    break;
            }
        }

        internal override ShaderParseResult Parse(RendererDX11 renderer, ShaderCompilerContext context)
        {
            ShaderParseResult result = new ShaderParseResult();
            Material material = new Material(renderer.Device, context.Filename);
            try
            {
                ParseHeader(material, context);
            }
            catch (Exception e)
            {
                result.Errors.Add($"{context.Filename ?? "Material header error"}: {e.Message}");
                return result;
            }

            // Proceed to compiling each material pass.
            MaterialPassCompileResult firstPassResult = null;
            foreach (MaterialPass pass in material.Passes)
            {
                MaterialPassCompileResult passResult = CompilePass(pass, context);
                firstPassResult = firstPassResult ?? passResult;

                result.Warnings.AddRange(passResult.Warnings);

                if (passResult.Errors.Count > 0)
                {
                    result.Errors.AddRange(passResult.Errors);
                    return result;
                }
            }

            // Populate metadata
            material.HasCommonConstants = HasConstantBuffer(material, result, CONST_COMMON_NAME, CONST_COMMON_VAR_NAMES);
            material.HasObjectConstants = HasConstantBuffer(material, result, CONST_OBJECT_NAME, CONST_OBJECT_VAR_NAMES);
            bool hasDiffuse = HasResource(material, MAP_DIFFUSE);
            bool hasNormal = HasResource(material, MAP_NORMAL);
            bool hasEmissive = HasResource(material, MAP_EMISSIVE);
            material.HasGBufferTextures = hasDiffuse && hasNormal && hasEmissive;

            // Validate the vertex input structure of all passes. Should match structure of first pass.
            // Only run this if there is more than 1 pass.
            if (material.PassCount > 1)
            {
                ShaderIOStructure iStructure = material.Passes[0].VertexShader.InputStructure;
                for (int i = 1; i < material.PassCount; i++)
                {
                    if (!material.Passes[i].VertexShader.InputStructure.IsCompatible(iStructure))
                    {
                        result.Errors.Add($"Vertex input structure in Pass #{i + 1} in material '{material.Name}' does not match structure of pass #1");
                        break;
                    }
                }
            }

            // No issues arose, lets add it to the material manager
            if (result.Errors.Count == 0)
            {
                material.InputStructure = material.Passes[0].VertexShader.InputStructure;
                material.InputStructureByteCode = firstPassResult.VertexResult.Bytecode;
                result.Shaders.Add(material);
                renderer.Materials.AddMaterial(material);

                if (material.HasCommonConstants)
                {
                    material.View = material["view"];
                    material.Projection = material["projection"];
                    material.ViewProjection = material["viewProjection"];
                    material.InvViewProjection = material["invViewProjection"];
                }

                if (material.HasObjectConstants)
                {
                    material.World = material["world"];
                    material.Wvp = material["wvp"];
                }

                if (material.HasGBufferTextures)
                {
                    material.DiffuseTexture = material["mapDiffuse"];
                    material.NormalTexture = material["mapNormal"];
                    material.EmissiveTexture = material["mapEmissive"];
                }
            }

            return result;
        }

        private MaterialPassCompileResult CompilePass(MaterialPass pass, ShaderCompilerContext context)
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
                BuildPassStructure(result);

            return result;
        }

        private void BuildPassStructure(MaterialPassCompileResult pResult)
        {
            MaterialPass pass = pResult.Pass;
            Material material = pass.Material as Material;
            GraphicsDevice device = material.Device;

            // Vertex Shader
            if (pResult.VertexResult != null)
            {
                if (!BuildStructure(material, pResult.VertexReflection, pResult.VertexResult, pass.VertexShader))
                    pResult.Errors.Add("Invalid vertex shader structure.");
            }

            // Hull Shader
            if (pResult.HullResult != null)
            {
                if (!BuildStructure(material, pResult.HullReflection, pResult.HullResult, pass.HullShader))
                    pResult.Errors.Add("Invalid hull shader structure.");
            }

            // Domain Shader
            if (pResult.DomainResult != null)
            {
                if (!BuildStructure(material, pResult.DomainReflection, pResult.DomainResult, pass.DomainShader))
                    pResult.Errors.Add("Invalid domain shader structure.");
            }

            // Geometry Shader
            if (pResult.GeometryResult != null)
            {
                if (!BuildStructure(material, pResult.GeometryReflection, pResult.GeometryResult, pass.GeometryShader))
                    pResult.Errors.Add("Invalid geometry shader structure.");
            }

            // PixelShader Shader
            if (pResult.PixelResult != null)
            {
                if (!BuildStructure(material, pResult.PixelReflection, pResult.PixelResult, pass.PixelShader))
                    pResult.Errors.Add("Invalid pixel shader structure.");
            }
        }

        protected override void OnBuildVariableStructure(HlslShader shader, ShaderReflection reflection, InputBindingDescription binding, ShaderInputType inputType) { }
    }
}
