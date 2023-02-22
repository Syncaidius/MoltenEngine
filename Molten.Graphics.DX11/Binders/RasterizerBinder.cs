namespace Molten.Graphics
{
    internal unsafe class RasterizerBinder : GraphicsSlotBinder<RasterizerStateDX11>
    {
        public override void Bind(GraphicsSlot<RasterizerStateDX11> slot, RasterizerStateDX11 value)
        {
            CommandQueueDX11 cmd = slot.Cmd as CommandQueueDX11;

            if (value == null)
            {
                PipelineStateDX11 state = cmd.Device.StatePresets.Default as PipelineStateDX11;
                value = state.RasterizerState;
            }

            cmd.Native->RSSetState(value as RasterizerStateDX11);
        }

        public override void Unbind(GraphicsSlot<RasterizerStateDX11> slot, RasterizerStateDX11 value)
        {
            (slot.Cmd as CommandQueueDX11).Native->RSSetState(null);
        }
    }
}
