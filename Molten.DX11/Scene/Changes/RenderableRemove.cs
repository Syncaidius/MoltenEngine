using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="SceneObject"/> to the root of a scene.</summary>
    internal class RenderableRemove : RenderSceneChange<RenderableRemove>
    {
        public Renderable Renderable;

        public ObjectRenderData Data;

        public override void Clear()
        {
            Renderable = null;
            Data = null;
        }

        public override void Process(SceneRenderDataDX11 scene)
        {
            List<ObjectRenderData> dataList;
            if (scene.Renderables.TryGetValue(Renderable, out dataList))
                dataList.Remove(Data);

            Recycle(this);
        }
    }
}
