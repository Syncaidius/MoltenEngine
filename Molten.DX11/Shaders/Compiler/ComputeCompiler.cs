using Silk.NET.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class ComputeCompiler : HlslSubCompiler
    {
        internal override List<IShader> Parse(ShaderCompilerContext context, RendererDX11 renderer, string header)
        {
            List<IShader> result = new List<IShader>();
            ComputeTask compute = new ComputeTask(renderer.Device, context.Filename);
            try
            {
                context.Compiler.ParserHeader(compute, ref header, context);
                CompilationResult computeResult = null;
                if (Compile(compute.Composition.EntryPoint, ShaderType.ComputeShader, context, out computeResult))
                {
                    ShaderReflection shaderRef = new ShaderReflection(computeResult.Bytecode);
                    if (BuildStructure(context, compute, shaderRef, computeResult, compute.Composition))
                    {
                        compute.Composition.RawShader = new ComputeShader(renderer.Device.D3d, computeResult.Bytecode);
                    }
                }
            }
            catch (Exception e)
            {
                context.Errors.Add($"{context.Filename ?? "Material header error"}: {e.Message}");
            }

            if(context.Errors.Count == 0)
            {
                result.Add(compute);
                (renderer.Compute as ComputeManager).AddTask(compute);
            }

            return result;
        }

        protected override void OnBuildVariableStructure(ShaderCompilerContext context, HlslShader shader, ShaderReflection reflection, InputBindingDescription binding, ShaderInputType inputType)
        {
            ComputeTask ct = shader as ComputeTask;

            switch (inputType)
            {
                case D3DShaderInputType.D3D11SitUavRwstructured:
                    if (ct != null)
                        OnBuildRWStructuredVariable(context, ct, binding);
                    break;

                case D3DShaderInputType.D3D11SitUavRwtyped:
                    if (ct != null)
                        OnBuildRWTypedVariable(context, ct, binding);
                    break;
            }
        }

        protected void OnBuildRWStructuredVariable(ShaderCompilerContext context, ComputeTask shader, InputBindingDescription binding)
        {
            RWBufferVariable rwBuffer = GetVariableResource<RWBufferVariable>(context, shader, binding);
            int bindPoint = binding.BindPoint;

            if (bindPoint >= shader.UAVs.Length)
                Array.Resize(ref shader.UAVs, bindPoint + 1);

            shader.UAVs[bindPoint] = rwBuffer;
        }

        protected void OnBuildRWTypedVariable(ShaderCompilerContext context, ComputeTask shader, InputBindingDescription binding)
        {
            RWVariable resource = null;
            int bindPoint = binding.BindPoint;

            switch (binding.Dimension)
            {
                case D3DSrvDimension.D3D101SrvDimensionTexture1D:
                    resource = GetVariableResource<RWTexture1DVariable>(context, shader, binding);
                    break;

                case D3DSrvDimension.D3D101SrvDimensionTexture2D:
                    resource = GetVariableResource<RWTexture2DVariable>(context, shader, binding);
                    break;
            }

            if (bindPoint >= shader.UAVs.Length)
                Array.Resize(ref shader.UAVs, bindPoint + 1);

            // Store the resource variable
            shader.UAVs[bindPoint] = resource;
        }
    }
}
