namespace Molten.Graphics;

public class GpuObjectException : Exception
{
    public GpuObjectException(GpuObject obj, string message) : base(message)
    {
        Object = obj;
    }

    public GpuObject Object { get; private set; }
}
