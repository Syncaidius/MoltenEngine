namespace Molten.Graphics
{
    public abstract class GraphicsSlotBinder<T>
        where T : class, IGraphicsObject
    {
        public abstract void Bind(GraphicsSlot<T> slot, T value);

        public abstract void Unbind(GraphicsSlot<T> slot, T value);
    }
}
