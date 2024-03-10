using Molten.Collections;

namespace Molten.Graphics;

public abstract class GraphicsResourceTask<R> : GraphicsTask
    where R : GpuResource
{
    /// <summary>
    /// The target <see cref="GpuResource"/>. This is set by the <see cref="RenderService"/> when the task is queued.
    /// </summary>
    public R Resource;
}
