namespace Molten.Graphics
{
    internal unsafe class DepthStencilBinder : GraphicsSlotBinder<DepthStateDX11>
    {
        public override void Bind(GraphicsSlot<DepthStateDX11> slot, DepthStateDX11 value)
        {
            (slot.Cmd as GraphicsQueueDX11).Native->OMSetDepthStencilState(value.NativePtr, value.StencilReference);
        }

        public override void Unbind(GraphicsSlot<DepthStateDX11> slot, DepthStateDX11 value)
        {
            GraphicsQueueDX11 cmd = slot.Cmd as GraphicsQueueDX11;
            cmd.Native->OMSetDepthStencilState(null, 0);
        }
    }
}
