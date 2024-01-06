namespace Molten.Graphics;

internal class RunResourceTask<T> : RenderTask<RunResourceTask<T>>
    where T : struct, IGraphicsResourceTask
{
    internal T Task;

    internal GraphicsResource Resource;

    public override void ClearForPool()
    {
        Resource = null;
        Task = default;
    }

    public override void Process(RenderService renderer)
    {
        if (Task.Process(renderer.Device.Queue, Resource))
            Resource.Version++;

        Recycle(this);
    }
}
