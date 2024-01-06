namespace Molten.Graphics;

public class GraphicsDeviceException : Exception
{
    public GraphicsDeviceException(GraphicsDevice device, string message) : base(message)
    {
        Device = device;
    }

    public GraphicsDevice Device { get; private set; }
}
