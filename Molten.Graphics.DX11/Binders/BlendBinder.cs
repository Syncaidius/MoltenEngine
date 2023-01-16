namespace Molten.Graphics
{
    internal unsafe class BlendBinder : ContextSlotBinder<GraphicsBlendState>
    {
        internal override void Bind(ContextSlot<GraphicsBlendState> slot, GraphicsBlendState value)
        {
            value = value ?? slot.CmdList.DXDevice.BlendBank.GetPreset(BlendPreset.Default);
            Color4 tmp = value.BlendFactor;
            slot.CmdList.Native->OMSetBlendState(value, (float*)&tmp, value.BlendSampleMask);
        }

        internal override void Unbind(ContextSlot<GraphicsBlendState> slot, GraphicsBlendState value)
        {
            slot.CmdList.Native->OMSetBlendState(null, null, 0);
        }
    }
}
