namespace Molten.Graphics;

/// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="IRenderable"/> to the root of a scene.</summary>
internal class RenderableAdd : RenderSceneChange<RenderableAdd>
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

    public override void Process()
    {
        RenderDataBatch batch;
        if (!LayerData.Renderables.TryGetValue(Renderable, out batch))
        {
            batch = new RenderDataBatch();
            LayerData.Renderables.Add(Renderable, batch);
        }

        batch.Add(Data);
        Recycle(this);
    }
}
