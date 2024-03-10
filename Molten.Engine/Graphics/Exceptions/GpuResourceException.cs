namespace Molten.Graphics;

public class GpuResourceException : Exception
{
    public GpuResourceException(GpuResource resource, string message) : base(message)
    {
        Resource = resource;
    }

    public GpuResource Resource { get; }

    public GpuResourceFlags Flags => Resource?.Flags ?? GpuResourceFlags.None;
}
