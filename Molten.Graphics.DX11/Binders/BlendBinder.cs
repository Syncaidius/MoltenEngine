namespace Molten.Graphics
{
    internal unsafe class BlendBinder : GraphicsSlotBinder<BlendStateDX11>
    {
        public override void Bind(GraphicsSlot<BlendStateDX11> slot, BlendStateDX11 value)
        {
            CommandQueueDX11 cmd = slot.Cmd as CommandQueueDX11;

            if (value == null)
            {
                PipelineStateDX11 state = cmd.Device.DefaultState as PipelineStateDX11;
                value = state.BlendState;
            }

            Color4 tmp = value.BlendFactor;
            cmd.Native->OMSetBlendState(value, (float*)&tmp, value.BlendSampleMask);
        }

        public override void Unbind(GraphicsSlot<BlendStateDX11> slot, BlendStateDX11 value)
        {
            (slot.Cmd as CommandQueueDX11).Native->OMSetBlendState(null, null, 0);
        }
    }
}
