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
                ShaderClassResult result = null;
                if (context.Renderer.ShaderCompiler.CompileSource(compute.Composition.EntryPoint, ShaderType.Compute, context, out result))
                {
                    if(BuildStructure(context, compute, result, compute.Composition))
                        compute.Composition.SetBytecode((ID3D10Blob*)result.ByteCode);
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
            HlslFoundation shader, ShaderClassResult result, ShaderResourceInfo info)
        {
            ComputeTask ct = shader as ComputeTask;
            if (ct == null)
                return;

            switch (info.Type)
            {
                case ShaderInputType.UavRWStructured:
                        OnBuildRWStructuredVariable(context, ct, info);
                    break;

                case ShaderInputType.UavRWTyped:
                        OnBuildRWTypedVariable(context, ct, info);
                    break;
            }
        }

        protected void OnBuildRWStructuredVariable
            (ShaderCompilerContext<RendererDX11, HlslFoundation> context, 
            ComputeTask shader, ShaderResourceInfo info)
        {
            RWBufferVariable rwBuffer = GetVariableResource<RWBufferVariable>(context, shader, info);
            uint bindPoint = info.BindPoint;

            if (bindPoint >= shader.UAVs.Length)
                EngineUtil.ArrayResize(ref shader.UAVs, bindPoint + 1);

            shader.UAVs[bindPoint] = rwBuffer;
        }

        protected void OnBuildRWTypedVariable(
            ShaderCompilerContext<RendererDX11, HlslFoundation> context, 
            ComputeTask shader, ShaderResourceInfo info)
        {
            RWVariable resource = null;
            uint bindPoint = info.BindPoint;

            switch (info.Dimension)
            {
                case ShaderResourceDimension.Texture1D:
                    resource = GetVariableResource<RWTexture1DVariable>(context, shader, info);
                    break;

                case ShaderResourceDimension.Texture2D:
                    resource = GetVariableResource<RWTexture2DVariable>(context, shader, info);
                    break;
            }

            if (bindPoint >= shader.UAVs.Length)
                EngineUtil.ArrayResize(ref shader.UAVs, bindPoint + 1);

            // Store the resource variable
            shader.UAVs[bindPoint] = resource;
        }
    }
}
