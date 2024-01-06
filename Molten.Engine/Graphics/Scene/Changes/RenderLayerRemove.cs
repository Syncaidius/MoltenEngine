namespace Molten.Graphics;

/// <summary>A <see cref="RenderLayerAdd"/> for adding <see cref="LayerRenderData"/> to the a<see cref="SceneRenderData"/> instance.</summary>
internal class RenderLayerRemove : RenderSceneChange<RenderLayerRemove>
{
    public SceneRenderData SceneData;

    public LayerRenderData LayerData;

    public override void ClearForPool()
    {
        SceneData = null;
        LayerData = null;
    }

    public override void Process()
    {
        SceneData.Layers.Remove(LayerData);
        Recycle(this);
    }
}
