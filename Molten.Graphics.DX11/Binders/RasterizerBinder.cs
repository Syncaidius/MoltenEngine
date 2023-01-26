namespace Molten.Graphics
{
    internal unsafe class RasterizerBinder : GraphicsSlotBinder<RasterizerStateDX11>
    {
        public override void Bind(GraphicsSlot<RasterizerStateDX11> slot, RasterizerStateDX11 value)
        {
            CommandQueueDX11 cmd = slot.Cmd as CommandQueueDX11;
            value = value ?? cmd.DXDevice.RasterizerBank.GetPreset(RasterizerPreset.Default);
            cmd.Native->RSSetState(value);
        }

        public override void Unbind(GraphicsSlot<RasterizerStateDX11> slot, RasterizerStateDX11 value)
        {
            (slot.Cmd as CommandQueueDX11).Native->RSSetState(null);
        }
    }
}
