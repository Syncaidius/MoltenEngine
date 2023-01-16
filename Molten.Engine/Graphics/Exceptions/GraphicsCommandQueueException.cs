namespace Molten.Graphics
{
    public class GraphicsCommandQueueException : Exception
    {
        public GraphicsCommandQueueException(GraphicsCommandQueue queue, string message) : base(message)
        {
            Queue = queue;
        }

        public GraphicsCommandQueue Queue { get; }
    }
}
