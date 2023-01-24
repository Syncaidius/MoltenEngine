using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class SurfaceGroupBinder : ContextGroupBinder<RenderSurface2D>
    {
        internal override void Bind(ContextSlotGroup<RenderSurface2D> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            
        }

        internal override void Bind(ContextSlot<RenderSurface2D> slot, RenderSurface2D value)
        {
            
        }

        internal override void Unbind(ContextSlotGroup<RenderSurface2D> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            uint numRTs = endIndex + 1;
            var rtvs = grp.Cmd.RTVs;
            for (uint i = 0; i < numRTs; i++)
                rtvs[i] = null;

            grp.Cmd.Native->OMSetRenderTargets(numRTs, (ID3D11RenderTargetView**)rtvs, grp.Cmd.DSV);
        }

        internal override void Unbind(ContextSlot<RenderSurface2D> slot, RenderSurface2D value)
        {
            var rtvs = slot.Cmd.RTVs;
            rtvs[slot.SlotIndex] = null;
            slot.Cmd.Native->OMSetRenderTargets(1, (ID3D11RenderTargetView**)rtvs, slot.Cmd.DSV);
        }
    }
}
