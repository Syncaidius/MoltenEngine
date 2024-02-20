namespace Molten.Graphics;

public unsafe abstract class GraphicsResourceHandle : IDisposable
{
    public abstract void Dispose();

    protected GraphicsResourceHandle(GraphicsResource resource)
    {
        Resource = resource;
    }

    /// <summary>
    /// Gets the <see cref="GraphicsResource"/> that this handle is associated with.
    /// </summary>
    public GraphicsResource Resource { get; }
}
