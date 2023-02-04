namespace Molten.Graphics
{
    internal unsafe class InputLayoutBinder : GraphicsSlotBinder<VertexInputLayout>
    {
        public override void Bind(GraphicsSlot<VertexInputLayout> slot, VertexInputLayout value)
        {
            CommandQueueDX11 cmd = (slot.Cmd as CommandQueueDX11);
            if (value != null && value.IsNullBuffer)
                cmd.Native->IASetInputLayout(null);
            else
                cmd.Native->IASetInputLayout(value);
        }

        public override void Unbind(GraphicsSlot<VertexInputLayout> slot, VertexInputLayout value)
        {
            (slot.Cmd as CommandQueueDX11).Native->IASetInputLayout(null);
        }
    }
}
