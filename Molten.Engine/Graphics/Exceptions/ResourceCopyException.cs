namespace Molten.Graphics;

public class ResourceCopyException : Exception
{
    public ResourceCopyException(GraphicsResource source, GraphicsResource destination)
    : this(source, destination, "Invalid copy operation.")
    { }

    public ResourceCopyException(GraphicsResource source, GraphicsResource destination, string message)
        : base(message)
    {
        Source = source;
        Destination = destination;
    }

    /// <summary>
    /// The source texture.
    /// </summary>
    public GraphicsResource Source { get; private set; }

    /// <summary>
    /// The destination texture.
    /// </summary>
    public GraphicsResource Destination { get; private set; }
}
