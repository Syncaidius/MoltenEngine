namespace Molten.Graphics;

/// <summary>A <see cref="RenderLayerReorder"/> for changing the draw order of a <see cref="LayerRenderData"/> instance.</summary>
internal class RenderLayerReorder : GpuTask
{
    public LayerRenderData LayerData;
    public SceneRenderData SceneData;
    public ReorderMode Mode;

    public override void ClearForPool()
    {
        LayerData = null;
        SceneData = null;
    }

    public override bool Validate() => true;

    protected override bool OnProcess(RenderService renderer, GpuCommandList cmd)
    {
        int indexOf = SceneData.Layers.IndexOf(LayerData);
        if (indexOf > -1)
        {
            SceneData.Layers.RemoveAt(indexOf);

            switch (Mode)
            {
                case ReorderMode.PushBackward:
                    SceneData.Layers.Insert(Math.Max(0, indexOf - 1), LayerData);
                    break;

                case ReorderMode.BringToFront:
                    SceneData.Layers.Add(LayerData);
                    break;

                case ReorderMode.PushForward:
                    if (indexOf + 1 < SceneData.Layers.Count)
                        SceneData.Layers.Insert(indexOf + 1, LayerData);
                    else
                        SceneData.Layers.Add(LayerData);
                    break;

                case ReorderMode.SendToBack:
                    SceneData.Layers.Insert(0, LayerData);
                    break;
            }
        }

        return true;
    }
}
