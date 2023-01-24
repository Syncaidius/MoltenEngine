namespace Molten.Graphics
{
    internal unsafe class BlendBinder : ContextSlotBinder<GraphicsBlendState>
    {
        internal override void Bind(ContextSlot<GraphicsBlendState> slot, GraphicsBlendState value)
        {
            value = value ?? slot.Cmd.DXDevice.BlendBank.GetPreset(BlendPreset.Default);
            Color4 tmp = value.BlendFactor;
            slot.Cmd.Native->OMSetBlendState(value, (float*)&tmp, value.BlendSampleMask);
        }

        internal override void Unbind(ContextSlot<GraphicsBlendState> slot, GraphicsBlendState value)
        {
            slot.Cmd.Native->OMSetBlendState(null, null, 0);
        }
    }
}
