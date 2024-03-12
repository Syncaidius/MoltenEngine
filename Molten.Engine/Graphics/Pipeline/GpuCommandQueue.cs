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
    /// <param name="cmd"></param>
    public abstract void Execute(GpuCommandList cmd);

    /// <summary>
    /// Resets the provided <see cref="GpuCommandList"/> so that it can be re-used.
    /// </summary>
    /// <param name="cmd"></param>
    public abstract void Reset(GpuCommandList cmd);

    /// <summary>
    /// Forces a CPU-side wait on the current thread until the provided <see cref="GpuFence"/> is signaled by the GPU.
    /// </summary>
    /// <param name="fence">The fence to wait on.</param>
    /// <param name="nsTimeout">An optional timeout, in nanoseconds.</param>
    /// <returns></returns>
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