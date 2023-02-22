namespace Molten.Graphics
{
    internal unsafe class StateBinder : GraphicsSlotBinder<GraphicsPipelineState>
    {
        public override void Bind(GraphicsSlot<GraphicsPipelineState> slot, GraphicsPipelineState value) { }

        public override void Unbind(GraphicsSlot<GraphicsPipelineState> slot, GraphicsPipelineState value) { }
    }
}
