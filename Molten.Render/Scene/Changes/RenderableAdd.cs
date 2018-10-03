using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="IRenderable"/> to the root of a scene.</summary>
    internal class RenderableAdd<R> : RenderSceneChange<RenderableAdd<R>> 
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
            if (!LayerData.Renderables.TryGetValue(Renderable, out dataList))
            {
                dataList = new List<ObjectRenderData>();
                LayerData.Renderables.Add(Renderable, dataList);
            }

            dataList.Add(Data);
            Recycle(this);
        }
    }
}
