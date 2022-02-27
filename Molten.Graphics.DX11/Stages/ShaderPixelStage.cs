using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class ShaderPixelStage : PipeShaderStage<ID3D11PixelShader>
    {
        public ShaderPixelStage(DeviceContext pipe) :
            base(pipe, ShaderType.PixelShader)
        {

        }

        protected override void OnUnbindConstBuffer(PipeSlot<ShaderConstantBuffer> slot)
        {
            ID3D11Buffer** nullbuffer = stackalloc ID3D11Buffer*[1];
            nullbuffer[0] = null;

            Pipe.NativeContext->PSSetConstantBuffers(slot.Index, 1, nullbuffer);
        }

        protected override void OnUnbindResource(PipeSlot<PipeBindableResource> slot)
        {
            ID3D11ShaderResourceView** nullSrv = stackalloc ID3D11ShaderResourceView*[1];
            nullSrv[0] = null;

            Pipe.NativeContext->PSSetShaderResources(slot.Index, 1, nullSrv);
        }

        protected override void OnUnbindSampler(PipeSlot<ShaderSampler> slot)
        {
            ID3D11SamplerState** nullState = stackalloc ID3D11SamplerState*[1];
            nullState[0] = null;

            Pipe.NativeContext->PSSetSamplers(slot.Index, 1, nullState);
        }

        protected override void OnUnbindShaderComposition(PipeSlot<ShaderComposition<ID3D11PixelShader>> slot)
        {
            Pipe.NativeContext->PSSetShader(null, null, 0);
        }

        protected override unsafe void OnBindConstants(PipeSlotGroup<ShaderConstantBuffer> grp,
            ID3D11Buffer** buffers)
        {
            Pipe.NativeContext->PSSetConstantBuffers(grp.FirstChanged, grp.NumSlotsChanged, buffers);
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
                Pipe.NativeContext->PSSetShader(slot.BoundValue.PtrShader, null, 0);
            else
                Pipe.NativeContext->PSSetShader(null, null, 0);
        }
    }
}
