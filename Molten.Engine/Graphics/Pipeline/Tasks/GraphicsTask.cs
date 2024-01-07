using Molten.Collections;

namespace Molten.Graphics;

public abstract class GraphicsTask : IPoolable
{
    internal ObjectPool<GraphicsTask> Pool { get; set; }

    internal void Recycle()
    {
        Pool.Recycle(this);
    }

    public abstract void ClearForPool();

    /// <summary>
    /// Invoked when the current <see cref="GraphicsTask"/> needs to be processed.
    /// </summary>
    /// <param name="renderer">The <see cref="RenderService"/> that the task is bound to.</param>
    /// <param name="queue">The <see cref="GraphicsQueue"/> that should process the task.</param>
    public abstract void Process(RenderService renderer, GraphicsQueue queue);
}
