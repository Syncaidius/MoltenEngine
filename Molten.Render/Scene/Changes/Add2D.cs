using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="SceneObject"/> to the root of a scene.</summary>
    internal class Add2D : RenderSceneChange<Add2D> 
    {
        public IRenderable2D Object;
        public LayerRenderData LayerData;

        public override void Clear()
        {
            Object = null;
            LayerData = null;
        }

        public override void Process()
        {
            LayerData.Renderables2D.Add(Object);
            Recycle(this);
        }
    }
}
