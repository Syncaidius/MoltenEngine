namespace Molten.Graphics
{
    internal unsafe class BlendBinder : GraphicsSlotBinder<BlendStateDX11>
    {
        public override void Bind(GraphicsSlot<BlendStateDX11> slot, BlendStateDX11 value)
        {
            GraphicsQueueDX11 cmd = slot.Cmd as GraphicsQueueDX11;
            Color4 tmp = value.BlendFactor;
            cmd.Ptr->OMSetBlendState(value, (float*)&tmp, value.BlendSampleMask);
        }

        public override void Unbind(GraphicsSlot<BlendStateDX11> slot, BlendStateDX11 value)
        {
            (slot.Cmd as GraphicsQueueDX11).Ptr->OMSetBlendState(null, null, 0);
        }
    }
}
