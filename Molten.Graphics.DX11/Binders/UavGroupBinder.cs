using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class UavGroupBinder : ContextGroupBinder<ContextBindableResource>
    {
        ShaderCSStage _stage;

        internal UavGroupBinder(ShaderCSStage stage)
        {
            _stage = stage;
        }

        internal override void Bind(ContextSlotGroup<ContextBindableResource> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            // Set unordered access resources
            ID3D11UnorderedAccessView** pUavs = stackalloc ID3D11UnorderedAccessView*[(int)numChanged];
            uint* pInitialCounts = stackalloc uint[(int)numChanged];

            uint sid = startIndex;
            for (int i = 0; i < numChanged; i++)
            {
                pUavs[i] = grp[sid].BoundValue != null ? grp[sid].BoundValue.UAV.NativePtr : null;
                pInitialCounts[i] = 0; // TODO set initial counts. Research this more.
                sid++;
            }

            _stage.SetUnorderedAccessViews(startIndex, numChanged, pUavs, pInitialCounts);
        }

        internal override void Bind(ContextSlot<ContextBindableResource> slot, ContextBindableResource value)
        {
            ID3D11UnorderedAccessView** pUavs = stackalloc ID3D11UnorderedAccessView*[1];
            uint* pInitialCounts = stackalloc uint[1];
            pUavs[0] = slot.BoundValue != null ? slot.BoundValue.UAV.NativePtr : null;
            pInitialCounts[0] = 0;
            _stage.SetUnorderedAccessViews(slot.SlotIndex, 1, pUavs, pInitialCounts);
        }

        internal override void Unbind(ContextSlotGroup<ContextBindableResource> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            // Set unordered access resources
            ID3D11UnorderedAccessView** pUavs = stackalloc ID3D11UnorderedAccessView*[(int)numChanged];
            uint* pInitialCounts = stackalloc uint[(int)numChanged];

            uint sid = startIndex;
            for (int i = 0; i < numChanged; i++)
            {
                pUavs[i] = null;
                pInitialCounts[i] = 0; // TODO set initial counts. Research this more.
            }

            _stage.SetUnorderedAccessViews(startIndex, numChanged, pUavs, pInitialCounts);
        }

        internal override void Unbind(ContextSlot<ContextBindableResource> slot, ContextBindableResource value)
        {
            ID3D11UnorderedAccessView** pUavs = stackalloc ID3D11UnorderedAccessView*[1];
            uint* pInitialCounts = stackalloc uint[1];
            pUavs[0] = null;
            pInitialCounts[0] = 0;
            _stage.SetUnorderedAccessViews(slot.SlotIndex, 1, pUavs, pInitialCounts);
        }
    }
}
