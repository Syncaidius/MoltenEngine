using Molten.Collections;

namespace Molten.Graphics;

public abstract class GraphicsTask : IPoolable
{
    public delegate void EventHandler(GraphicsTask task, bool success);

    /// <summary>
    /// Gets or sets the pool that owns the current <see cref="GraphicsTask"/>.
    /// </summary>
    internal ObjectPool<GraphicsTask> Pool { get; set; }

    /// <summary>
    /// Invoked when the task has been completed.
    /// </summary>
    public event EventHandler OnCompleted;

    public abstract void ClearForPool();

    public abstract bool Validate();

    /// <summary>
    /// Invoked when the current <see cref="GraphicsTask"/> needs to be processed.
    /// </summary>
    /// <param name="cmd">The <see cref="GpuCommandList"/> that should process the task.</param>
    public void Process(GpuCommandList cmd)
    {
        if(OnProcess(cmd.Device.Renderer, cmd))
            OnCompleted?.Invoke(this, true);
        else
            OnCompleted?.Invoke(this, false);

        OnCompleted = null;

        // Recycle the completed/failed task.
        Pool.Recycle(this);
    }

    /// <summary>
    /// Invoked when the task should be processed by the specified <see cref="GpuCommandQueue"/>.
    /// </summary>
    /// <param name="renderer">The renderer that the task is bound to.</param>
    /// <param name="cmd">The <see cref="GpuCommandQueue"/> that should process the current <see cref="GraphicsTask"/>.</param>
    /// <returns></returns>
    protected abstract bool OnProcess(RenderService renderer, GpuCommandList cmd);
}
