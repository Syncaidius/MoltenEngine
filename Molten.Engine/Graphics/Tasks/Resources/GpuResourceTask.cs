namespace Molten.Graphics;

public abstract class GpuResourceTask<R> : GpuTask
    where R : GpuResource
{
    /// <summary>
    /// The target <see cref="GpuResource"/>. This is set by the <see cref="RenderService"/> when the task is queued.
    /// </summary>
    public R Resource;
}
