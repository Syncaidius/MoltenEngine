using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class SurfaceGroupBinder : GraphicsGroupBinder<IRenderSurface2D>
    {
        public override void Bind(GraphicsSlotGroup<IRenderSurface2D> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            
        }

        public override void Bind(GraphicsSlot<IRenderSurface2D> slot, IRenderSurface2D value)
        {
            
        }

        public override void Unbind(GraphicsSlotGroup<IRenderSurface2D> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            CommandQueueDX11 cmd = grp.Cmd as CommandQueueDX11;

            uint numRTs = endIndex + 1;
            var rtvs = cmd.RTVs;
            for (uint i = 0; i < numRTs; i++)
                rtvs[i] = null;

            cmd.Native->OMSetRenderTargets(numRTs, (ID3D11RenderTargetView**)rtvs, cmd.DSV);
        }

        public override void Unbind(GraphicsSlot<IRenderSurface2D> slot, IRenderSurface2D value)
        {
            CommandQueueDX11 cmd = slot.Cmd as CommandQueueDX11;

            var rtvs = cmd.RTVs;
            rtvs[slot.SlotIndex] = null;
            cmd.Native->OMSetRenderTargets(1, (ID3D11RenderTargetView**)rtvs, cmd.DSV);
        }
    }
}
