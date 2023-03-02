namespace Molten.Graphics
{
    internal unsafe class ComputeCompiler : FxcCodeCompiler
    {
        public override ShaderCodeType ClassType => ShaderCodeType.Compute;

        public override List<HlslElement> Build(
            ShaderCompilerContext context, 
            RenderService renderer, in string header)
        {
            List<HlslElement> shaders = new List<HlslElement>();
            FxcCompiler fxc = context.Compiler as FxcCompiler;

            ComputeTask compute = new ComputeTask(renderer.Device, context.Source.Filename);
            try
            {
                context.Compiler.ParserHeader(compute, in header, context);
                ShaderCodeResult result = null;
                if (fxc.CompileSource(compute.Composition.EntryPoint, ShaderType.Compute, context, out result))
                {
                    if(BuildStructure(context, compute, result, compute.Composition))
                        compute.Composition.BuildShader(result.ByteCode);
                }
            }
            catch (Exception e)
            {
                context.AddError($"{context.Source.Filename ?? "Material header error"}: {e.Message}");
            }

            if(!context.HasErrors)
            {
                shaders.Add(compute);
                renderer.Compute.AddTask(compute);
            }

            // Intialize the shader's default resource array, now that we have the final count of the shader's actual resources.
            foreach (HlslShader shader in shaders)
                shader.DefaultResources = new IShaderResource[shader.Resources.Length];

            return shaders;
        }

        protected override void OnBuildVariableStructure(
            ShaderCompilerContext context,
            HlslElement shader, ShaderCodeResult result, ShaderResourceInfo info)
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
            (ShaderCompilerContext context, 
            ComputeTask shader, ShaderResourceInfo info)
        {
            RWBufferVariable rwBuffer = GetVariableResource<RWBufferVariable>(context, shader, info);
            uint bindPoint = info.BindPoint;

            if (bindPoint >= shader.UAVs.Length)
                EngineUtil.ArrayResize(ref shader.UAVs, bindPoint + 1);

            shader.UAVs[bindPoint] = rwBuffer;
        }

        protected void OnBuildRWTypedVariable(
            ShaderCompilerContext context, 
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
