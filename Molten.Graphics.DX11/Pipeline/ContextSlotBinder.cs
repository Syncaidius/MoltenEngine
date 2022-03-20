namespace Molten.Graphics
{
    internal abstract class ContextSlotBinder<T> 
        where T: ContextBindable
    {
        internal abstract void Bind(ContextSlot<T> slot, T value);

        internal abstract void Unbind(ContextSlot<T> slot, T value);
    }
}
