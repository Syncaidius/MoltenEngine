using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class ShaderPixelStage : PipeShaderStage<ID3D11PixelShader>
    {
        public ShaderPixelStage(DeviceContext pipe) :
            base(pipe, ShaderType.DomainShader)
        {

        }

        protected override unsafe void OnBindConstants(PipeSlotGroup<ShaderConstantBuffer> grp,
            ID3D11Buffer** buffers, uint* firstConstants, uint* numConstants)
        {
            Pipe.NativeContext->PSSetConstantBuffers1(grp.FirstChanged, grp.NumSlotsChanged, buffers, firstConstants, numConstants);
        }

        protected override unsafe void OnBindResources(PipeSlotGroup<PipeBindableResource> grp,
            ID3D11ShaderResourceView** srvs)
        {
            Pipe.NativeContext->PSSetShaderResources(grp.FirstChanged, grp.NumSlotsChanged, srvs);
        }

        protected override unsafe void OnBindSamplers(PipeSlotGroup<ShaderSampler> grp, ID3D11SamplerState** resources)
        {
            Pipe.NativeContext->PSSetSamplers(grp.FirstChanged, grp.NumSlotsChanged, resources);
        }

        protected override unsafe void OnBindShader(PipeSlot<ShaderComposition<ID3D11PixelShader>> slot)
        {
            if (slot.BoundValue != null)
                Pipe.NativeContext->PSSetShader(slot.BoundValue.RawShader, null, 0);
            else
                Pipe.NativeContext->PSSetShader(null, null, 0);
        }
    }
}
