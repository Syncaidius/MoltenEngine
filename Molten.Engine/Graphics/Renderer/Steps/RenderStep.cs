namespace Molten.Graphics;

/// <summary>
/// A base class for render steps.
/// </summary>
internal abstract class RenderStep : IDisposable
{
    internal void Initialize(RenderService renderer)
    {
        Renderer = renderer;
        OnInitialize(renderer);
    }

    protected abstract void OnInitialize(RenderService service);

    internal abstract void Draw(GpuCommandList cmd, RenderCamera camera, RenderChainContext context, Timing time);

    public abstract void Dispose();

    internal RenderService Renderer { get; private set; }
}
