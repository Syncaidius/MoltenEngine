using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class ShaderCSStage : ContextShaderStage<ID3D11ComputeShader>
    {
        public ShaderCSStage(CommandQueueDX11 queue) : base(queue, ShaderType.Compute)
        {
            uint uavSlots = queue.DXDevice.Adapter.Capabilities.UnorderedAccessBuffers.MaxSlots;
            UAVs = queue.RegisterSlotGroup(GraphicsBindTypeFlags.Input, "UAV", uavSlots, new UavGroupBinder(this));
        }

        internal override bool Bind()
        {
            bool uavChanged = UAVs.BindAll();
            bool baseChanged =  base.Bind();

            return uavChanged || baseChanged;
        }

        internal override unsafe void SetConstantBuffers(uint startSlot, uint numBuffers, ID3D11Buffer** buffers)
        {
            Cmd.Native->CSSetConstantBuffers(startSlot, numBuffers, buffers);
        }

        internal override unsafe void SetResources(uint startSlot, uint numViews, ID3D11ShaderResourceView1** views)
        {
            Cmd.Native->CSSetShaderResources(startSlot, numViews, (ID3D11ShaderResourceView**)views);
        }

        internal override unsafe void SetSamplers(uint startSlot, uint numSamplers, ID3D11SamplerState** states)
        {
            Cmd.Native->CSSetSamplers(startSlot, numSamplers, states);
        }

        internal override unsafe void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances)
        {
            Cmd.Native->CSSetShader((ID3D11ComputeShader*)shader, classInstances, numClassInstances);
        }

        internal unsafe void SetUnorderedAccessViews(uint startSlot, uint numUAVs, ID3D11UnorderedAccessView1** ppUnorderedAccessViews, uint* pUAVInitialCounts)
        {
            Cmd.Native->CSSetUnorderedAccessViews(startSlot, numUAVs, (ID3D11UnorderedAccessView**)ppUnorderedAccessViews, pUAVInitialCounts);
        }

        protected override unsafe void GetResources(uint startSlot, uint numViews, ID3D11ShaderResourceView** ptrViews)
        {
            Cmd.Native->CSGetShaderResources(startSlot, numViews, ptrViews);
        }

        protected override unsafe void GetShader(void** shader, ID3D11ClassInstance** classInstances, uint* numClassInstances)
        {
            Cmd.Native->CSGetShader((ID3D11ComputeShader**)shader, classInstances, numClassInstances);
        }

        internal GraphicsSlotGroup<GraphicsResourceDX11> UAVs { get; }
    }
}
