namespace Molten.Graphics;

public abstract class GpuFence : GpuObject
{
    protected GpuFence(GpuDevice device) : base(device) { }

    /// <summary>
    /// Halts execution on the current thread until the fence is signaled by the GPU.
    /// </summary>
    /// <param name="nsTimeout">A timeout, in nanoseconds. If set to 0, the call will immediately return the fence status as a bool without waiting.</param>
    /// <returns>True if the wait was succesful, or false if the timeout was reached.</returns>
    public abstract bool Wait(ulong nsTimeout = ulong.MaxValue);

    public abstract void Reset();
}

/// <summary>
/// A graphics fence that is always signaled. This can be used as a placeholder for when a fence is not required, or when fences are not supported.
/// </summary>
public class GraphicsOpenFence : GpuFence
{
    public GraphicsOpenFence(GpuDevice device) : base(device) { }

    public override bool Wait(ulong nsTimeout = ulong.MaxValue)
    {
        return true;
    }

    public override void Reset() { }

    protected override void OnGraphicsRelease() { }
}
