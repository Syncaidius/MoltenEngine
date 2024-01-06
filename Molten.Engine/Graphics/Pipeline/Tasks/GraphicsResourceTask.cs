using Molten.Collections;

namespace Molten.Graphics;

public abstract class GraphicsResourceTask<R> : GraphicsTask
    where R : GraphicsResource
{

    /// <summary>
    /// The target <see cref="GraphicsResource"/>. This is set by the <see cref="RenderService"/> when the task is queued.
    /// </summary>
    public R Resource;

    public override void Process(RenderService renderer, GraphicsDevice taskDevice)
    {
        if (OnProcess(renderer.Device.Queue))
            Resource.Version++;
    }

    /// <summary>
    /// Invoked when the current <see cref="GraphicsResourceTask{R}"/> needs to be processed.
    /// </summary>
    /// <param name="queue"></param>
    /// <returns>True if the <see cref="GraphicsResource"/> was altered.</returns>
    protected abstract bool OnProcess(GraphicsQueue queue);

    public abstract void Validate();
}
