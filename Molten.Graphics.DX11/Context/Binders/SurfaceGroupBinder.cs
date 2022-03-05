using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class SurfaceGroupBinder : ContextGroupBinder<RenderSurface>
    {
        internal override void Bind(ContextSlotGroup<RenderSurface> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            
        }

        internal override void Bind(ContextSlot<RenderSurface> slot, RenderSurface value)
        {
            
        }

        internal override void Unbind(ContextSlotGroup<RenderSurface> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            uint numRTs = endIndex + 1;
            var rtvs = grp.Context.State.RTVs;
            for (uint i = 0; i < numRTs; i++)
                rtvs[i] = null;

            grp.Context.Native->OMGetRenderTargets(numRTs, rtvs, ref grp.Context.State.DSV);
        }

        internal override void Unbind(ContextSlot<RenderSurface> slot, RenderSurface value)
        {
            var rtvs = slot.Context.State.RTVs;
            rtvs[slot.SlotIndex] = null;
            slot.Context.Native->OMGetRenderTargets(1, rtvs, ref slot.Context.State.DSV);
        }
    }
}
