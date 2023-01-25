namespace Molten.Graphics
{
    internal unsafe class ComputeTaskBinder : GraphicsSlotBinder<ComputeTask>
    {
        public override void Bind(GraphicsSlot<ComputeTask> slot, ComputeTask value) { }

        public override void Unbind(GraphicsSlot<ComputeTask> slot, ComputeTask value) { }
    }
}
