namespace Molten.Graphics
{
    internal unsafe class ComputeBuilder : ShaderCodeCompiler
    {
        public override ShaderCodeType ClassType => ShaderCodeType.Compute;

        public override List<HlslGraphicsObject> Build(
            ShaderCompilerContext context, 
            RenderService renderer, in string header)
        {
            List<HlslGraphicsObject> result = new List<HlslGraphicsObject>();
            ComputeTask task = new ComputeTask(renderer.Device, context.Source.Filename);

            try
            {
                context.Compiler.ParserHeader(task, in header, context);

                if (task.Passes == null || task.Passes.Length == 0)
                {
                    context.AddError($"Compute task '{task.Name}' is invalid: No passes defined");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(task.Passes[0].Composition.EntryPoint))
                    {
                        context.AddError($"Compute task '{task.Name} is invalid: First pass must define an entry point");
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                context.AddError($"{context.Source.Filename ?? "Compute task header error"}: {e.Message}");
                renderer.Device.Log.Error(e);
                return result;
            }

            // Proceed to compiling each compute pass.
            PassCompileResult firstPassResult = null;
            foreach (ComputePass pass in task.Passes)
            {
                PassCompileResult passResult = CompilePass(context, pass);
                firstPassResult = firstPassResult ?? passResult;

                if (context.HasErrors)
                    return result;
            }

            if (!context.HasErrors)
            {
                result.Add(task);
                renderer.Compute.AddTask(task);
            }

            // Intialize the shader's default resource array, now that we have the final count of the shader's actual resources.
            foreach (HlslShader shader in result)
                shader.DefaultResources = new IShaderResource[shader.Resources.Length];

            return result;
        }

        private PassCompileResult CompilePass(
            ShaderCompilerContext context,
            ComputePass pass)
        {
            PassCompileResult result = new PassCompileResult(pass);
            ShaderComposition sc = pass.Composition;

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

            return result;
        }
    }
}
