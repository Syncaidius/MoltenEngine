namespace Molten.Graphics
{
    internal unsafe class InputLayoutBinder : GraphicsSlotBinder<VertexInputLayout>
    {
        public override void Bind(GraphicsSlot<VertexInputLayout> slot, VertexInputLayout value)
        {
            (slot.Cmd as CommandQueueDX11).Native->IASetInputLayout(slot.BoundValue);
        }

        public override void Unbind(GraphicsSlot<VertexInputLayout> slot, VertexInputLayout value)
        {
            (slot.Cmd as CommandQueueDX11).Native->IASetInputLayout(null);
        }
    }
}
