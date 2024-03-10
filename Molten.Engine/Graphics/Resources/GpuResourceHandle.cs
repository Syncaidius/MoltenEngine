namespace Molten.Graphics;

public unsafe abstract class GpuResourceHandle : IDisposable
{
    public abstract void Dispose();

    protected GpuResourceHandle(GpuResource resource)
    {
        Resource = resource;
    }

    /// <summary>
    /// Gets the <see cref="GpuResource"/> that this handle is associated with.
    /// </summary>
    public GpuResource Resource { get; }
}
