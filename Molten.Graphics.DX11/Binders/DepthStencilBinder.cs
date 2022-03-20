namespace Molten.Graphics
{
    internal unsafe class DepthStencilBinder : ContextSlotBinder<GraphicsDepthState>
    {
        internal override void Bind(ContextSlot<GraphicsDepthState> slot, GraphicsDepthState value)
        {
            value = value ?? slot.Context.Device.DepthBank.GetPreset(DepthStencilPreset.Default);
            slot.Context.Native->OMSetDepthStencilState(value.NativePtr, value.StencilReference);
        }

        internal override void Unbind(ContextSlot<GraphicsDepthState> slot, GraphicsDepthState value)
        {
            slot.Context.Native->OMSetDepthStencilState(null, 0);
        }
    }
}
