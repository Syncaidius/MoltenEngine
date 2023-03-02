namespace Molten.Graphics
{
    internal unsafe class ComputeBuilder : ShaderCodeCompiler
    {
        public override ShaderCodeType ClassType => ShaderCodeType.Compute;

        public override List<HlslElement> Build(
            ShaderCompilerContext context, 
            RenderService renderer, in string header)
        {
            List<HlslElement> shaders = new List<HlslElement>();

            ComputeTask compute = new ComputeTask(renderer.Device, context.Source.Filename);
            try
            {
                context.Compiler.ParserHeader(compute, in header, context);
                ShaderCodeResult result = null;
                if (context.Compiler.CompileSource(compute.Composition.EntryPoint, ShaderType.Compute, context, out result))
                {
                    if (context.Compiler.BuildStructure(context, compute, result, compute.Composition))
                        compute.Composition.PtrShader = context.Compiler.BuildShader(compute, ShaderType.Compute, result.ByteCode);
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
    }
}
