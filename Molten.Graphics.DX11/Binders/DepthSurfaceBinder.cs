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
            GraphicsQueueDX11 cmd = slot.Cmd as GraphicsQueueDX11;

            uint maxRTs = cmd.Device.Capabilities.PixelShader.MaxOutputTargets;
            cmd.DSV = null;
            cmd.Ptr->OMSetRenderTargets(maxRTs, (ID3D11RenderTargetView**)cmd.RTVs, cmd.DSV);
        }
    }
}
