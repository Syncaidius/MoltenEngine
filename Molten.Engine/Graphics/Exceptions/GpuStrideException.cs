namespace Molten.Graphics;
public class GpuStrideException : Exception
{
    public GpuStrideException(uint stride, string message) : base(message)
    {
        Stride = stride;
    }

    public uint Stride { get; }
}
