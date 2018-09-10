using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class SceneLayerData
    {
        public readonly List<IRenderable2D> Renderables2D = new List<IRenderable2D>();
    }

    public class SceneLayerData<R> : SceneLayerData
        where R : IRenderable3D
    {
        public Dictionary<R, List<ObjectRenderData>> Renderables = new Dictionary<R, List<ObjectRenderData>>();
    }
}
