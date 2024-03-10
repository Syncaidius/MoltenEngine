namespace Molten.Graphics;

public class ResourceCopyException : Exception
{
    public ResourceCopyException(GpuResource source, GpuResource destination)
    : this(source, destination, "Invalid copy operation.")
    { }

    public ResourceCopyException(GpuResource source, GpuResource destination, string message)
        : base(message)
    {
        Source = source;
        Destination = destination;
    }

    /// <summary>
    /// The source texture.
    /// </summary>
    public GpuResource Source { get; private set; }

    /// <summary>
    /// The destination texture.
    /// </summary>
    public GpuResource Destination { get; private set; }
}
