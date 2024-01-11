namespace Molten.Graphics;
public class GraphicsStrideException : Exception
{
    public GraphicsStrideException(uint stride, string message) : base(message)
    {
        Stride = stride;
    }

    public uint Stride { get; }
}
