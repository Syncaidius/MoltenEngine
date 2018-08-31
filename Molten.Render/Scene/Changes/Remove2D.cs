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
        public SceneRenderData Data;

        public override void Clear()
        {
            Object = null;
            Data = null;
        }

        public override void Process()
        {
            Data.Renderables2D.Remove(Object);
            Recycle(this);
        }
    }
}
