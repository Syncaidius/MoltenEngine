using Molten.Collections;

namespace Molten.Graphics;

public abstract class GraphicsResourceTask<R> : GraphicsTask
    where R : GraphicsResource
{
    /// <summary>
    /// The target <see cref="GraphicsResource"/>. This is set by the <see cref="RenderService"/> when the task is queued.
    /// </summary>
    public R Resource;

    public abstract void Validate();
}
