using System.Collections.Generic;

namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for removing a <see cref="IRenderable"/> from the root of a scene.</summary>
    internal class RenderableRemove<R> : RenderSceneChange<RenderableRemove<R>>
        where R : class, IRenderable
    {
        public R Renderable;

        public ObjectRenderData Data;

        public LayerRenderData<R> LayerData;

        public override void Clear()
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
