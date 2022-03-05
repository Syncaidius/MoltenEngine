using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class DepthSurfaceBinder : ContextSlotBinder<DepthStencilSurface>
    {
        internal override void Bind(ContextSlot<DepthStencilSurface> slot, DepthStencilSurface value)
        {
            
        }

        internal override void Unbind(ContextSlot<DepthStencilSurface> slot, DepthStencilSurface value)
        {
            uint maxRTs = slot.Context.Device.Features.SimultaneousRenderSurfaces;
            slot.Context.State.DSV = null;
            slot.Context.Native->OMGetRenderTargets(maxRTs, slot.Context.State.RTVs, ref slot.Context.State.DSV);
        }
    }
}
