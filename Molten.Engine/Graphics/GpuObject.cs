namespace Molten.Graphics;

public abstract class GpuObject : EngineObject
{
    protected GpuObject(GpuDevice device)
    {
        Device = device;
    }

    protected override sealed void OnDispose(bool immediate)
    {
        if (immediate)
            GraphicsRelease();
        else
            Device.MarkForRelease(this);
    }

    internal void GraphicsRelease()
    {
        if (IsReleased)
            throw new GpuObjectException(this, "The current GraphicsObject is already released");

        OnGraphicsRelease();
        IsReleased = true;
    }

    /// <summary>
    /// Invoked when the object should release any graphics resources.
    /// </summary>
    protected abstract void OnGraphicsRelease();

    /// <summary>
    /// Gets the <see cref="GpuDevice"/> that the current <see cref="GpuObject"/> is bound to.
    /// </summary>
    public GpuDevice Device { get; }

    /// <summary>
    /// Gets the instance-specific version of the current <see cref="GpuObject"/>. Any change which will require a device
    /// update should increase this value. E.g. Resizing a texture, recompiling a shader/material, etc.
    /// </summary>
    public uint Version { get; set; }

    /// <summary>
    /// Gets whether or not the current <see cref="GpuObject"/> has been successfully disposed and released by its parent <see cref="Device"/>.
    /// </summary>
    public bool IsReleased { get; private set; }

    /// <summary>
    /// Gets the frame ID that the current <see cref="GpuObject"/> was initially marked for release.
    /// </summary>
    internal ulong ReleaseFrameID { get; set; }
}

/// <summary>
/// A generic version of <see cref="GpuObject"/> which is bound to a specific type of <see cref="GpuDevice"/>.
/// </summary>
/// <typeparam name="T">The type of <see cref="GpuDevice"/> to bind to.</typeparam>
public abstract class GraphicsObject<T> : GpuObject
    where T : GpuDevice
{
    protected GraphicsObject(T device) : base(device)
    {
        Device = device;
    }

    /// <summary>
    /// Gets the <typeparamref name="T"/> that the current <see cref="GpuObject"/> is bound to.
    /// </summary>
    public new T Device { get; }
}
