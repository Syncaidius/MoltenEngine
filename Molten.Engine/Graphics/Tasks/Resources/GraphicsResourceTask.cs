using Molten.Collections;

namespace Molten.Graphics;

public abstract class GraphicsResourceTask<R> : GpuTask
    where R : GpuResource
{
    /// <summary>
    /// The target <see cref="GpuResource"/>. This is set by the <see cref="RenderService"/> when the task is queued.
    /// </summary>
    public R Resource;
}
