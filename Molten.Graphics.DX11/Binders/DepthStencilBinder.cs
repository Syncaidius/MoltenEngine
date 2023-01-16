namespace Molten.Graphics
{
    internal unsafe class DepthStencilBinder : ContextSlotBinder<GraphicsDepthState>
    {
        internal override void Bind(ContextSlot<GraphicsDepthState> slot, GraphicsDepthState value)
        {
            value = value ?? slot.CmdList.DXDevice.DepthBank.GetPreset(DepthStencilPreset.Default);
            slot.CmdList.Native->OMSetDepthStencilState(value.NativePtr, value.StencilReference);
        }

        internal override void Unbind(ContextSlot<GraphicsDepthState> slot, GraphicsDepthState value)
        {
            slot.CmdList.Native->OMSetDepthStencilState(null, 0);
        }
    }
}
