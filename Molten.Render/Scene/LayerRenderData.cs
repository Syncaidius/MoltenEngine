using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class LayerRenderData
    {
        public readonly List<IRenderable2D> Renderables2D = new List<IRenderable2D>();
    }

    public class SceneLayerData<R, C> : LayerRenderData
        where R : IRenderable3D
        where C: IRenderChain
    {
        public readonly Dictionary<R, List<ObjectRenderData>> Renderables = new Dictionary<R, List<ObjectRenderData>>();

        public C RenderChain { get; set; }
    }
}
