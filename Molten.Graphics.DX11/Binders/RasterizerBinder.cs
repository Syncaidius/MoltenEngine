namespace Molten.Graphics
{
    internal unsafe class RasterizerBinder : ContextSlotBinder<GraphicsRasterizerState>
    {
        internal override void Bind(ContextSlot<GraphicsRasterizerState> slot, GraphicsRasterizerState value)
        {
            value = value ?? slot.Cmd.DXDevice.RasterizerBank.GetPreset(RasterizerPreset.Default);
            slot.Cmd.Native->RSSetState(value);
        }

        internal override void Unbind(ContextSlot<GraphicsRasterizerState> slot, GraphicsRasterizerState value)
        {
            slot.Cmd.Native->RSSetState(null);
        }
    }
}
