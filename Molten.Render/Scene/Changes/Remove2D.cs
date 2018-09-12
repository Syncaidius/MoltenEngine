using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="IRenderable2D"/> to the root of a scene.</summary>
    internal class Remove2D : RenderSceneChange<Remove2D> 
    {
        public IRenderable2D Object;
        public LayerRenderData Layerdata;

        public override void Clear()
        {
            Object = null;
            Layerdata = null;
        }

        public override void Process()
        {
            Layerdata.Renderables2D.Remove(Object);
            Recycle(this);
        }
    }
}
