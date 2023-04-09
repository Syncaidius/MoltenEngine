namespace Molten.Graphics
{
    internal unsafe class RasterizerBinder : GraphicsSlotBinder<RasterizerStateDX11>
    {
        public override void Bind(GraphicsSlot<RasterizerStateDX11> slot, RasterizerStateDX11 value)
        {
            (slot.Cmd as GraphicsQueueDX11).Native->RSSetState(value);
        }

        public override void Unbind(GraphicsSlot<RasterizerStateDX11> slot, RasterizerStateDX11 value)
        {
            (slot.Cmd as GraphicsQueueDX11).Native->RSSetState(null);
        }
    }
}
