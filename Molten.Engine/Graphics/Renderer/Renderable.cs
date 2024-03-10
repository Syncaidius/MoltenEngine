namespace Molten.Graphics;

public abstract class Renderable
{
    protected Renderable(RenderService renderer)
    {
        Renderer = renderer;
        IsVisible = false;
    }

    internal bool BatchRender(GpuCommandQueue cmd, RenderService renderer, RenderCamera camera, RenderDataBatch batch)
    {
        return OnBatchRender(cmd, renderer, camera, batch);
    }

    internal void Render(GpuCommandQueue cmd, RenderService renderer, RenderCamera camera, ObjectRenderData data)
    {
        OnRender(cmd, renderer, camera, data);
    }

    protected virtual bool OnBatchRender(GpuCommandQueue cmd, RenderService renderer, RenderCamera camera, RenderDataBatch batch)
    {
        return false;
    }

    protected abstract void OnRender(GpuCommandQueue cmd, RenderService renderer, RenderCamera camera, ObjectRenderData data);

    /// <summary>Gets or sets whether or not the renderable should be drawn.</summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// Gets the renderer that owns the current <see cref="Renderable"/>.
    /// </summary>
    internal RenderService Renderer { get; private set; }
}
