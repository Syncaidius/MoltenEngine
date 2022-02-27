using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class ShaderVertexStage : PipeShaderStage<ID3D11VertexShader>
    {
        public ShaderVertexStage(DeviceContext pipe) :
            base(pipe, ShaderType.VertexShader)
        {
        }

        protected override void OnUnbindConstBuffer(PipeSlot<ShaderConstantBuffer> slot)
        {
            Pipe.NativeContext->VSSetConstantBuffers(slot.Index, 1, null);
        }

        protected override void OnUnbindResource(PipeSlot<PipeBindableResource> slot)
        {
            Pipe.NativeContext->VSSetShaderResources(slot.Index, 1, null);
        }

        protected override void OnUnbindSampler(PipeSlot<ShaderSampler> slot)
        {
            Pipe.NativeContext->VSSetSamplers(slot.Index, 1, null);
        }

        protected override void OnUnbindShaderComposition(PipeSlot<ShaderComposition<ID3D11VertexShader>> slot)
        {
            Pipe.NativeContext->VSSetShader(null, null, 0);
        }

        protected override unsafe void OnBindConstants(PipeSlotGroup<ShaderConstantBuffer> grp,
            ID3D11Buffer** buffers)
        {
            Pipe.NativeContext->VSSetConstantBuffers(grp.FirstChanged, grp.NumSlotsChanged, buffers);
        }

        protected override unsafe void OnBindResources(PipeSlotGroup<PipeBindableResource> grp,
            ID3D11ShaderResourceView** srvs)
        {
            Pipe.NativeContext->VSSetShaderResources(grp.FirstChanged, grp.NumSlotsChanged, srvs);
        }

        protected override unsafe void OnBindSamplers(PipeSlotGroup<ShaderSampler> grp, ID3D11SamplerState** resources)
        {
            Pipe.NativeContext->VSSetSamplers(grp.FirstChanged, grp.NumSlotsChanged, resources);
        }

        protected override unsafe void OnBindShader(PipeSlot<ShaderComposition<ID3D11VertexShader>> slot)
        {
            if (slot.BoundValue != null)
                Pipe.NativeContext->VSSetShader(slot.BoundValue.PtrShader, null, 0);
            else
                Pipe.NativeContext->VSSetShader(null, null, 0);
        }
    }
}
