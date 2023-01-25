namespace Molten.Graphics
{
    internal unsafe class RasterizerBinder : GraphicsSlotBinder<GraphicsRasterizerState>
    {
        public override void Bind(GraphicsSlot<GraphicsRasterizerState> slot, GraphicsRasterizerState value)
        {
            CommandQueueDX11 cmd = slot.Cmd as CommandQueueDX11;
            value = value ?? cmd.DXDevice.RasterizerBank.GetPreset(RasterizerPreset.Default);
            cmd.Native->RSSetState(value);
        }

        public override void Unbind(GraphicsSlot<GraphicsRasterizerState> slot, GraphicsRasterizerState value)
        {
            (slot.Cmd as CommandQueueDX11).Native->RSSetState(null);
        }
    }
}
