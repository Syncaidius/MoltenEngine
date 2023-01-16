namespace Molten.Graphics
{
    internal unsafe class RasterizerBinder : ContextSlotBinder<GraphicsRasterizerState>
    {
        internal override void Bind(ContextSlot<GraphicsRasterizerState> slot, GraphicsRasterizerState value)
        {
            value = value ?? slot.CmdList.DXDevice.RasterizerBank.GetPreset(RasterizerPreset.Default);
            slot.CmdList.Native->RSSetState(value);
        }

        internal override void Unbind(ContextSlot<GraphicsRasterizerState> slot, GraphicsRasterizerState value)
        {
            slot.CmdList.Native->RSSetState(null);
        }
    }
}
