using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class ShaderCSStage : ContextShaderStage<ID3D11ComputeShader>
    {
        public ShaderCSStage(DeviceContextState state) : base(state, ShaderType.Compute)
        {
            uint uavSlots = state.Context.Device.Adapter.Capabilities.UnorderedAccessBuffers.MaxSlots;
            UAVs = state.RegisterSlotGroup(ContextBindTypeFlags.Input, "UAV", uavSlots, new UavGroupBinder(this));
        }

        internal override bool Bind()
        {
            bool uavChanged = UAVs.BindAll();
            bool baseChanged =  base.Bind();

            return uavChanged || baseChanged;
        }

        internal override unsafe void SetConstantBuffers(uint startSlot, uint numBuffers, ID3D11Buffer** buffers)
        {
            Context.Native->CSSetConstantBuffers(startSlot, numBuffers, buffers);
        }

        internal override unsafe void SetResources(uint startSlot, uint numViews, ID3D11ShaderResourceView1** views)
        {
            Context.Native->CSSetShaderResources(startSlot, numViews, (ID3D11ShaderResourceView**)views);
        }

        internal override unsafe void SetSamplers(uint startSlot, uint numSamplers, ID3D11SamplerState** states)
        {
            Context.Native->CSSetSamplers(startSlot, numSamplers, states);
        }

        internal override unsafe void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances)
        {
            Context.Native->CSSetShader((ID3D11ComputeShader*)shader, classInstances, numClassInstances);
        }

        internal unsafe void SetUnorderedAccessViews(uint startSlot, uint numUAVs, ID3D11UnorderedAccessView1** ppUnorderedAccessViews, uint* pUAVInitialCounts)
        {
            Context.Native->CSSetUnorderedAccessViews(startSlot, numUAVs, (ID3D11UnorderedAccessView**)ppUnorderedAccessViews, pUAVInitialCounts);
        }

        internal ContextSlotGroup<ContextBindableResource> UAVs { get; }
    }
}
