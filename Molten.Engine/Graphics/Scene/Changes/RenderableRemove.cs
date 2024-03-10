namespace Molten.Graphics;

/// <summary>A <see cref="GraphicsTask"/> for removing a <see cref="Renderable"/> from the root of a scene.</summary>
internal class RenderableRemove : GraphicsTask
{
    public Renderable Renderable;

    public ObjectRenderData Data;

    public LayerRenderData LayerData;

    public override void ClearForPool()
    {
        Renderable = default;
        Data = null;
        LayerData = null;
    }

    public override bool Validate() => true;

    protected override bool OnProcess(RenderService renderer, GpuCommandQueue queue)
    {
        RenderDataBatch batch;
        if (LayerData.Renderables.TryGetValue(Renderable, out batch))
            batch.Remove(Data);
        else
            return false;

        return true;
    }
}
