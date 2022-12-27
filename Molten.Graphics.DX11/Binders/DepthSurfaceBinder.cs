using Silk.NET.Direct3D11;

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
            slot.Context.Native->OMSetRenderTargets(maxRTs, (ID3D11RenderTargetView**)slot.Context.State.RTVs, slot.Context.State.DSV);
        }
    }
}
