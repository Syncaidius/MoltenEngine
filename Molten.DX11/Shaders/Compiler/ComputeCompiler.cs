using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace Molten.Graphics
{
    internal class ComputeCompiler : HlslSubCompiler
    {
        internal ComputeCompiler(Logger log) : base(log)
        {
            AddParser<ShaderEntryParser>("entry");
        }

        internal override ShaderParseResult Parse(RendererDX11 renderer, ShaderCompilerContext context)
        {
            ShaderParseResult result = new ShaderParseResult();
            ComputeTask compute = new ComputeTask(renderer.Device, context.Filename);
            try
            {
                ParseHeader(compute, context.Header);
                CompilationResult computeResult = null;
                if (Compile(compute.Composition.EntryPoint, ShaderType.ComputeShader, context, out computeResult))
                {
                    ShaderReflection shaderRef = new ShaderReflection(computeResult.Bytecode);
                    if (BuildStructure(compute, shaderRef, computeResult, compute.Composition))
                    {
                        compute.Composition.RawShader = new ComputeShader(renderer.Device.D3d, computeResult.Bytecode);
                    }
                }
            }
            catch (Exception e)
            {
                result.Errors.Add($"{context.Filename ?? "Material header error"}: {e.Message}");
            }

            if(result.Errors.Count == 0)
            {
                result.Shaders.Add(compute);
                renderer.Compute.AddTask(compute);
            }

            return result;
        }

        protected override void OnBuildVariableStructure(HlslShader shader, ShaderReflection reflection, InputBindingDescription binding, ShaderInputType inputType)
        {
            ComputeTask ct = shader as ComputeTask;

            switch (inputType)
            {
                case ShaderInputType.UnorderedAccessViewRWStructured:
                    if (ct != null)
                        OnBuildRWStructuredVariable(ct, binding);
                    break;

                case ShaderInputType.UnorderedAccessViewRWTyped:
                    if (ct != null)
                        OnBuildRWTypedVariable(ct, binding);
                    break;
            }
        }

        protected void OnBuildRWStructuredVariable(ComputeTask shader, InputBindingDescription binding)
        {
            RWBufferVariable rwBuffer = GetVariableResource<RWBufferVariable>(shader, binding);
            int bindPoint = binding.BindPoint;

            if (bindPoint >= shader.UAVs.Length)
                Array.Resize(ref shader.UAVs, bindPoint + 1);

            shader.UAVs[bindPoint] = rwBuffer;
        }

        protected void OnBuildRWTypedVariable(ComputeTask shader, InputBindingDescription binding)
        {
            RWVariable resource = null;
            int bindPoint = binding.BindPoint;

            switch (binding.Dimension)
            {
                case ShaderResourceViewDimension.Texture1D:
                    resource = GetVariableResource<RWTexture1DVariable>(shader, binding);
                    break;

                case ShaderResourceViewDimension.Texture2D:
                    resource = GetVariableResource<RWTexture2DVariable>(shader, binding);
                    break;
            }

            if (bindPoint >= shader.UAVs.Length)
                Array.Resize(ref shader.UAVs, bindPoint + 1);

            // Store the resource variable
            shader.UAVs[bindPoint] = resource;
        }
    }
}
