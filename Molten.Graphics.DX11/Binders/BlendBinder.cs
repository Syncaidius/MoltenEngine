namespace Molten.Graphics
{
    internal unsafe class BlendBinder : GraphicsSlotBinder<GraphicsBlendState>
    {
        public override void Bind(GraphicsSlot<GraphicsBlendState> slot, GraphicsBlendState value)
        {
            CommandQueueDX11 cmd = slot.Cmd as CommandQueueDX11;

            value = value ?? cmd.DXDevice.BlendBank.GetPreset(BlendPreset.Default) as BlendStateDX11;
            Color4 tmp = value.BlendFactor;
            cmd.Native->OMSetBlendState(value as BlendStateDX11, (float*)&tmp, value.BlendSampleMask);
        }

        public override void Unbind(GraphicsSlot<GraphicsBlendState> slot, GraphicsBlendState value)
        {
            (slot.Cmd as CommandQueueDX11).Native->OMSetBlendState(null, null, 0);
        }
    }
}
