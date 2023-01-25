namespace Molten.Graphics
{
    internal unsafe class MaterialBinder : GraphicsSlotBinder<Material>
    {
        public override void Bind(GraphicsSlot<Material> slot, Material value) { }

        public override void Unbind(GraphicsSlot<Material> slot, Material value) { }
    }
}
