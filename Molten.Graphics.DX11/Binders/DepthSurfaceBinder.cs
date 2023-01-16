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
            uint maxRTs = slot.CmdList.DXDevice.Adapter.Capabilities.PixelShader.MaxOutResources;
            slot.CmdList.State.DSV = null;
            slot.CmdList.Native->OMSetRenderTargets(maxRTs, (ID3D11RenderTargetView**)slot.CmdList.State.RTVs, slot.CmdList.State.DSV);
        }
    }
}
