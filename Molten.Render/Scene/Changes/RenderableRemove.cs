using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="SceneObject"/> to the root of a scene.</summary>
    internal class RenderableRemove<R> : RenderSceneChange<RenderableRemove<R>>
        where R: class, IRenderable3D
    {
        public R Renderable;

        public ObjectRenderData Data;

        public SceneLayerData<R> LayerData;

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
