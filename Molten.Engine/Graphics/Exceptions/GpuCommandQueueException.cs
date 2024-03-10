namespace Molten.Graphics;

public class GpuCommandQueueException : Exception
{
    public GpuCommandQueueException(GpuCommandQueue queue, string message) : base(message)
    {
        Queue = queue;
    }

    public GpuCommandQueue Queue { get; }
}
