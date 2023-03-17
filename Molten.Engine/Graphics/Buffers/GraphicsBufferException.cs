namespace Molten.Graphics
{
    public class GraphicsBufferException : Exception
    {
        public GraphicsBufferException(IGraphicsBuffer buffer, string message) : base(message)
        {
            Buffer = buffer;
        }

        public IGraphicsBuffer Buffer { get; }

        public BufferFlags Flags => Buffer?.Flags ?? BufferFlags.None;
    }
}
