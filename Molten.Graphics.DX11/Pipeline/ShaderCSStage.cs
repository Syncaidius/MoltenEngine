using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class ShaderCSStage : ShaderStageDX11
    {
        public ShaderCSStage(CommandQueueDX11 queue) : base(queue, ShaderType.Compute)
        {
            uint uavSlots = queue.Device.Adapter.Capabilities.Compute.MaxUnorderedAccessSlots;
            UAVs = queue.RegisterSlotGroup(GraphicsBindTypeFlags.Output, "UAV", uavSlots, new UavGroupBinder(this));
        }

        internal override bool Bind()
        {
            bool baseChanged = base.Bind();
            bool uavChanged = false;

            ShaderComposition composition = Shader.BoundValue;

            // Apply unordered acces views to slots
            if (composition != null)
            {
                for (int j = 0; j < composition.UnorderedAccessIds.Count; j++)
                {
                    uint slotID = composition.UnorderedAccessIds[j];
                    UAVs[slotID].Value = composition.Pass.Parent.UAVs[slotID]?.Resource as GraphicsResourceDX11;
                }

                uavChanged = UAVs.BindAll();
            }
            else
            {
                // NOTE Unbind UAVs?
            }

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

        internal GraphicsSlotGroup<GraphicsResourceDX11> UAVs { get; }
    }
}
