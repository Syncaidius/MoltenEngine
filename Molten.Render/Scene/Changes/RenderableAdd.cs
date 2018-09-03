using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="SceneObject"/> to the root of a scene.</summary>
    internal class RenderableAdd<R> : RenderSceneChange<RenderableAdd<R>> 
        where R : class, IRenderable3D
    {
        public R Renderable;

        public ObjectRenderData Data;

        public SceneRenderData<R> SceneData;

        public override void Clear()
        {
            Renderable = default;
            Data = null;
            SceneData = null;
        }

        public override void Process()
        {
            List<ObjectRenderData> dataList;

            if (!SceneData.Renderables.TryGetValue(Renderable, out dataList))
            {
                dataList = new List<ObjectRenderData>();
                SceneData.Renderables.Add(Renderable, dataList);
            }

            dataList.Add(Data);
            Recycle(this);
        }
    }
}
