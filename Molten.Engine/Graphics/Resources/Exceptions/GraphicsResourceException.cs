namespace Molten.Graphics;

public class GraphicsResourceException : Exception
{
    public GraphicsResourceException(GraphicsResource resource, string message) : base(message)
    {
        Resource = resource;
    }

    public GraphicsResource Resource { get; }

    public GraphicsResourceFlags Flags => Resource?.Flags ?? GraphicsResourceFlags.None;
}
