using Silk.NET.Core.Native;

namespace Molten.Graphics
{
    internal unsafe class ComputeCompiler : FxcClassCompiler
    {
        public override ShaderClassType ClassType => ShaderClassType.Compute;

        public override List<IShaderElement> Parse(
            ShaderCompilerContext<RendererDX11, HlslFoundation> context, 
            RendererDX11 renderer, in string header)
        {
            List<IShaderElement> shaders = new List<IShaderElement>();
            ComputeTask compute = new ComputeTask(renderer.NativeDevice, context.Source.Filename);
            try
            {
                context.Compiler.ParserHeader(compute, in header, context);
                FxcCompileResult result = null;
                if (context.Renderer.ShaderCompiler.CompileSource(compute.Composition.EntryPoint, ShaderType.Compute, context, out result))
                {
                    if(BuildStructure(context, compute, result, compute.Composition))
                        compute.Composition.SetBytecode(result.ByteCode);
                }
            }
            catch (Exception e)
            {
                context.AddError($"{context.Source.Filename ?? "Material header error"}: {e.Message}");
            }

            if(!context.HasErrors)
            {
                shaders.Add(compute);
                (renderer.Compute as ComputeManager).AddTask(compute);
            }

            // Intialize the shader's default resource array, now that we have the final count of the shader's actual resources.
            foreach (HlslShader shader in shaders)
                shader.DefaultResources = new IShaderResource[shader.Resources.Length];

            return shaders;
        }

        protected override void OnBuildVariableStructure(
            ShaderCompilerContext<RendererDX11, HlslFoundation> context,
            HlslFoundation shader, FxcCompileResult result, HlslInputBindDescription bind)
        {
            ComputeTask ct = shader as ComputeTask;

            switch (bind.Ptr->Type)
            {
                case D3DShaderInputType.D3D11SitUavRwstructured:
                    if (ct != null)
                        OnBuildRWStructuredVariable(context, ct, bind);
                    break;

                case D3DShaderInputType.D3D11SitUavRwtyped:
                    if (ct != null)
                        OnBuildRWTypedVariable(context, ct, bind);
                    break;
            }
        }

        protected void OnBuildRWStructuredVariable
            (ShaderCompilerContext<RendererDX11, HlslFoundation> context, 
            ComputeTask shader, HlslInputBindDescription bind)
        {
            RWBufferVariable rwBuffer = GetVariableResource<RWBufferVariable>(context, shader, bind);
            uint bindPoint = bind.Ptr->BindPoint;

            if (bindPoint >= shader.UAVs.Length)
                EngineUtil.ArrayResize(ref shader.UAVs, bindPoint + 1);

            shader.UAVs[bindPoint] = rwBuffer;
        }

        protected void OnBuildRWTypedVariable(
            ShaderCompilerContext<RendererDX11, HlslFoundation> context, 
            ComputeTask shader, HlslInputBindDescription bind)
        {
            RWVariable resource = null;
            uint bindPoint = bind.Ptr->BindPoint;

            switch (bind.Ptr->Dimension)
            {
                case D3DSrvDimension.D3D101SrvDimensionTexture1D:
                    resource = GetVariableResource<RWTexture1DVariable>(context, shader, bind);
                    break;

                case D3DSrvDimension.D3D101SrvDimensionTexture2D:
                    resource = GetVariableResource<RWTexture2DVariable>(context, shader, bind);
                    break;
            }

            if (bindPoint >= shader.UAVs.Length)
                EngineUtil.ArrayResize(ref shader.UAVs, bindPoint + 1);

            // Store the resource variable
            shader.UAVs[bindPoint] = resource;
        }
    }
}
