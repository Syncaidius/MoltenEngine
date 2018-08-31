using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="SceneObject"/> to the root of a scene.</summary>
    internal class RenderableAdd : RenderSceneChange<RenderableAdd> 
    {
        public Renderable Renderable;

        public ObjectRenderData Data;

        public SceneRenderDataDX11 SceneData;

        public override void Clear()
        {
            Renderable = null;
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
