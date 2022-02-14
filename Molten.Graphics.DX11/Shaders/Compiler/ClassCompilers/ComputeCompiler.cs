using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class ComputeCompiler : FxcClassCompiler
    {
        public override ShaderClassType ClassType => ShaderClassType.Compute;

        public override List<HlslShader> Parse(
            ShaderCompilerContext<RendererDX11, HlslFoundation, FxcCompileResult> context, 
            RendererDX11 renderer, in string header)
        {
            List<HlslShader> shaders = new List<HlslShader>();
            ComputeTask compute = new ComputeTask(renderer.Device, context.Source.Filename);
            try
            {
                context.Compiler.ParserHeader(compute, in header, context);
                FxcCompileResult result = null;
                if (context.Compiler.CompileSource(compute.Composition.EntryPoint, ShaderType.ComputeShader, context, out result))
                {
                    if(BuildStructure(context, compute, result, compute.Composition))
                    {
                        void* byteCode = result.ByteCode->GetBufferPointer();
                        nuint numBytes = result.ByteCode->GetBufferSize();
                        ID3D11ClassLinkage* linkage = null;
                        renderer.Device.NativeDevice->CreateComputeShader(result.ByteCode, numBytes, linkage, ref compute.Composition.RawShader);
                    }
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
            ShaderCompilerContext<RendererDX11, HlslFoundation, FxcCompileResult> context,
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
            (ShaderCompilerContext<RendererDX11, HlslFoundation, FxcCompileResult> context, 
            ComputeTask shader, HlslInputBindDescription bind)
        {
            RWBufferVariable rwBuffer = GetVariableResource<RWBufferVariable>(context, shader, bind);
            uint bindPoint = bind.Ptr->BindPoint;

            if (bindPoint >= shader.UAVs.Length)
                EngineUtil.ArrayResize(ref shader.UAVs, bindPoint + 1);

            shader.UAVs[bindPoint] = rwBuffer;
        }

        protected void OnBuildRWTypedVariable(
            ShaderCompilerContext<RendererDX11, HlslFoundation, FxcCompileResult> context, 
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
