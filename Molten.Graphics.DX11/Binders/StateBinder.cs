namespace Molten.Graphics
{
    internal unsafe class StateBinder : GraphicsSlotBinder<GraphicsState>
    {
        public override void Bind(GraphicsSlot<GraphicsState> slot, GraphicsState value) { }

        public override void Unbind(GraphicsSlot<GraphicsState> slot, GraphicsState value) { }
    }
}
