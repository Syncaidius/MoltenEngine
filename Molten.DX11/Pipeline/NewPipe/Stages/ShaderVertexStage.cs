using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class ShaderVertexStage : PipeShaderStage
    {
        public ShaderVertexStage(PipeDX11 pipe) : 
            base(pipe, ShaderType.VertexShader)
        {
        }

        protected override unsafe void OnBindConstants(PipeSlotGroup<ShaderConstantBuffer> grp, 
            ID3D11Buffer** buffers, uint* firstConstants, uint* numConstants)
        {
            Pipe.Context->VSSetConstantBuffers1(grp.FirstChanged, grp.NumSlotsChanged, buffers, firstConstants, numConstants);
        }

        protected override unsafe void OnBindResources(PipeSlotGroup<PipeBindableResource> grp, 
            ID3D11ShaderResourceView** srvs)
        {
            Pipe.Context->VSSetShaderResources(grp.FirstChanged, grp.NumSlotsChanged, srvs);
        }
    }
}
