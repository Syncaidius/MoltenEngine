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
            uint maxRTs = slot.Cmd.DXDevice.Adapter.Capabilities.PixelShader.MaxOutResources;
            slot.Cmd.DSV = null;
            slot.Cmd.Native->OMSetRenderTargets(maxRTs, (ID3D11RenderTargetView**)slot.Cmd.RTVs, slot.Cmd.DSV);
        }
    }
}
