using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class DepthSurfaceBinder : GraphicsSlotBinder<IDepthStencilSurface>
    {
        public override void Bind(GraphicsSlot<IDepthStencilSurface> slot, IDepthStencilSurface value)
        {
            
        }

        public override void Unbind(GraphicsSlot<IDepthStencilSurface> slot, IDepthStencilSurface value)
        {
            CommandQueueDX11 cmd = slot.Cmd as CommandQueueDX11;

            uint maxRTs = cmd.Device.Adapter.Capabilities.PixelShader.MaxOutResources;
            cmd.DSV = null;
            cmd.Native->OMSetRenderTargets(maxRTs, (ID3D11RenderTargetView**)cmd.RTVs, cmd.DSV);
        }
    }
}
