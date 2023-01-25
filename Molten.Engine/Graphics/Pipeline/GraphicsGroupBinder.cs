namespace Molten.Graphics
{
    public abstract class GraphicsGroupBinder<T> : GraphicsSlotBinder<T>
        where T : GraphicsObject
    {
        public abstract void Bind(GraphicsSlotGroup<T> grp, uint startIndex, uint endIndex, uint numChanged);

        public abstract void Unbind(GraphicsSlotGroup<T> grp, uint startIndex, uint endIndex, uint numChanged);

        internal void UnbindAll(GraphicsSlotGroup<T> grp)
        {
            Unbind(grp, 0, grp.SlotCount - 1, grp.SlotCount);
        }
    }
}
