namespace Molten.Graphics
{
    public class GraphicsCommandQueueException : Exception
    {
        public GraphicsCommandQueueException(GraphicsQueue queue, string message) : base(message)
        {
            Queue = queue;
        }

        public GraphicsQueue Queue { get; }
    }
}
