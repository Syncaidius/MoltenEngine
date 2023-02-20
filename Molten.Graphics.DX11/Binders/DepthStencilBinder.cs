namespace Molten.Graphics
{
    internal unsafe class DepthStencilBinder : GraphicsSlotBinder<DepthStateDX11>
    {
        public override void Bind(GraphicsSlot<DepthStateDX11> slot, DepthStateDX11 value)
        {
            CommandQueueDX11 cmd = slot.Cmd as CommandQueueDX11;
            value = value ?? cmd.Device.DepthBank.GetPreset(DepthStencilPreset.Default);

            cmd.Native->OMSetDepthStencilState((value).NativePtr, cmd.State.Value.StencilReference);
        }

        public override void Unbind(GraphicsSlot<DepthStateDX11> slot, DepthStateDX11 value)
        {
            CommandQueueDX11 cmd = slot.Cmd as CommandQueueDX11;
            cmd.Native->OMSetDepthStencilState(null, 0);
        }
    }
}
