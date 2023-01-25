using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class DepthSurfaceBinder : GraphicsSlotBinder<DepthStencilSurface>
    {
        public override void Bind(GraphicsSlot<DepthStencilSurface> slot, DepthStencilSurface value)
        {
            
        }

        public override void Unbind(GraphicsSlot<DepthStencilSurface> slot, DepthStencilSurface value)
        {
            CommandQueueDX11 cmd = slot.Cmd as CommandQueueDX11;

            uint maxRTs = cmd.DXDevice.Adapter.Capabilities.PixelShader.MaxOutResources;
            cmd.DSV = null;
            cmd.Native->OMSetRenderTargets(maxRTs, (ID3D11RenderTargetView**)cmd.RTVs, cmd.DSV);
        }
    }
}
