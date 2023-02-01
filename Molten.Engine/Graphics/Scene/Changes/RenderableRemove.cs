namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for removing a <see cref="IRenderable"/> from the root of a scene.</summary>
    internal class RenderableRemove : RenderSceneChange<RenderableRemove>
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
            List<ObjectRenderData> dataList;
            if (LayerData.Renderables.TryGetValue(Renderable, out dataList))
                dataList.Remove(Data);

            Recycle(this);
        }
    }
}
