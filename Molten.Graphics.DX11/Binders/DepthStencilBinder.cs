namespace Molten.Graphics.DX11
{
    internal unsafe class DepthStencilBinder : GraphicsSlotBinder<DepthStateDX11>
    {
        public override void Bind(GraphicsSlot<DepthStateDX11> slot, DepthStateDX11 value)
        {
            (slot.Cmd as GraphicsQueueDX11).Ptr->OMSetDepthStencilState(value.NativePtr, value.StencilReference);
        }

        public override void Unbind(GraphicsSlot<DepthStateDX11> slot, DepthStateDX11 value)
        {
            GraphicsQueueDX11 cmd = slot.Cmd as GraphicsQueueDX11;
            cmd.Ptr->OMSetDepthStencilState(null, 0);
        }
    }
}
