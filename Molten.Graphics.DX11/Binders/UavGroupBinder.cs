using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class UavGroupBinder : GraphicsGroupBinder<ContextBindableResource>
    {
        ShaderCSStage _stage;

        internal UavGroupBinder(ShaderCSStage stage)
        {
            _stage = stage;
        }

        public override void Bind(GraphicsSlotGroup<ContextBindableResource> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            // Set unordered access resources
            ID3D11UnorderedAccessView1** pUavs = stackalloc ID3D11UnorderedAccessView1*[(int)numChanged];
            uint* pInitialCounts = stackalloc uint[(int)numChanged];

            uint sid = startIndex;
            for (int i = 0; i < numChanged; i++)
            {
                pUavs[i] = grp[sid].BoundValue != null ? grp[sid].BoundValue.UAV.Ptr : null;
                pInitialCounts[i] = 0; // TODO set initial counts. Research this more.
                sid++;
            }

            _stage.SetUnorderedAccessViews(startIndex, numChanged, pUavs, pInitialCounts);
        }

        public override void Bind(GraphicsSlot<ContextBindableResource> slot, ContextBindableResource value)
        {
            ID3D11UnorderedAccessView1** pUavs = stackalloc ID3D11UnorderedAccessView1*[1];
            uint* pInitialCounts = stackalloc uint[1];
            pUavs[0] = slot.BoundValue != null ? slot.BoundValue.UAV.Ptr : null;
            pInitialCounts[0] = 0;
            _stage.SetUnorderedAccessViews(slot.SlotIndex, 1, pUavs, pInitialCounts);
        }

        public override void Unbind(GraphicsSlotGroup<ContextBindableResource> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            // Set unordered access resources
            ID3D11UnorderedAccessView1** pUavs = stackalloc ID3D11UnorderedAccessView1*[(int)numChanged];
            uint* pInitialCounts = stackalloc uint[(int)numChanged];

            uint sid = startIndex;
            for (int i = 0; i < numChanged; i++)
            {
                pUavs[i] = null;
                pInitialCounts[i] = 0; // TODO set initial counts. Research this more.
            }

            _stage.SetUnorderedAccessViews(startIndex, numChanged, pUavs, pInitialCounts);
        }

        public override void Unbind(GraphicsSlot<ContextBindableResource> slot, ContextBindableResource value)
        {
            ID3D11UnorderedAccessView1** pUavs = stackalloc ID3D11UnorderedAccessView1*[1];
            uint* pInitialCounts = stackalloc uint[1];
            pUavs[0] = null;
            pInitialCounts[0] = 0;
            _stage.SetUnorderedAccessViews(slot.SlotIndex, 1, pUavs, pInitialCounts);
        }
    }
}
