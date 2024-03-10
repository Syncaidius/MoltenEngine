namespace Molten.Graphics;

public class GpuCommandListException : Exception
{
    public GpuCommandListException(GpuCommandList list, string message) : base(message)
    {
        List = list;
    }

    public GpuCommandList List { get; }
}
