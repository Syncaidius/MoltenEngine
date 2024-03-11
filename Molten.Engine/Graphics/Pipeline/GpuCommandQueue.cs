namespace Molten.Graphics;

public abstract class GpuCommandQueue : EngineObject
{
    /// <summary>Returns a new (or recycled) <see cref="GpuCommandList"/> which can be used to record GPU commands.</summary>
    /// <param name="flags">The flags to apply to the underlying command segment.</param>   
    public abstract GpuCommandList GetCommandList(GpuCommandListFlags flags = GpuCommandListFlags.None);

    public abstract void BeginFrame();

    public abstract void EndFrame();
    
    /// <summary>
    /// Executes the provided <see cref="GpuCommandList"/> on the current <see cref="GpuCommandQueue"/>.
    /// </summary>
    /// <param name="list"></param>
    public abstract void Execute(GpuCommandList list);

    /// <summary>
    /// Resets the provided <see cref="GpuCommandList"/> so that it can be re-used.
    /// </summary>
    /// <param name="list"></param>
    public abstract void Reset(GpuCommandList list);

    public abstract bool Wait(GpuFence fence, ulong nsTimeout = ulong.MaxValue);

    /// <summary>
    /// Gets the parent <see cref="GpuDevice"/> of the current <see cref="GpuCommandQueue"/>.
    /// </summary>
    public abstract GpuDevice Device { get; }
}

public abstract class GpuCommandQueue<T> : GpuCommandQueue
    where T : GpuDevice
{
    protected GpuCommandQueue(T device) 
    {
        Device = device;
    }

    /// <inheritdoc/>
    public override T Device { get; }
}