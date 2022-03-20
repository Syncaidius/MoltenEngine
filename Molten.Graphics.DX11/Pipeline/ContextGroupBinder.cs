namespace Molten.Graphics
{
    internal abstract class ContextGroupBinder<T> : ContextSlotBinder<T>
        where T : ContextBindable
    {
        internal abstract void Bind(ContextSlotGroup<T> grp, uint startIndex, uint endIndex, uint numChanged);

        internal abstract void Unbind(ContextSlotGroup<T> grp, uint startIndex, uint endIndex, uint numChanged);

        internal void UnbindAll(ContextSlotGroup<T> grp)
        {
            Unbind(grp, 0, grp.SlotCount - 1, grp.SlotCount);
        }
    }
}
