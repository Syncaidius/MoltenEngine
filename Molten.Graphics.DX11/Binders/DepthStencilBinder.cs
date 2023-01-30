namespace Molten.Graphics
{
    internal unsafe class DepthStencilBinder : GraphicsSlotBinder<GraphicsDepthState>
    {
        public override void Bind(GraphicsSlot<GraphicsDepthState> slot, GraphicsDepthState value)
        {
            CommandQueueDX11 cmd = slot.Cmd as CommandQueueDX11;
            value = value ?? cmd.Device.DepthBank.GetPreset(DepthStencilPreset.Default);

            cmd.Native->OMSetDepthStencilState((value as DepthStateDX11).NativePtr, value.StencilReference);
        }

        public override void Unbind(GraphicsSlot<GraphicsDepthState> slot, GraphicsDepthState value)
        {
            CommandQueueDX11 cmd = slot.Cmd as CommandQueueDX11;
            cmd.Native->OMSetDepthStencilState(null, 0);
        }
    }
}
