using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    internal class SceneRemoveObject : SceneChange<SceneRemoveObject>
    {
        internal ISceneObject Object;

        internal SceneLayer Layer;

        public override void Clear()
        {
            Object = null;
            Layer = null;
        }

        internal override void Process(Scene scene)
        {
            if (Object.Scene == scene && Object.Layer == Layer)
            {
                Layer.Objects.Remove(Object);

                if(Object is IUpdatable up)
                    Layer.Updatables.Remove(up);

                if (Object is ICursorAcceptor acceptor)
                    Layer.InputAcceptors.Remove(acceptor);

                if (Object is IRenderable2D r2d)
                {
                    Layer.Renderables2d.Remove(r2d);
                    Object.Scene.RenderData.RemoveObject(r2d, Layer.Data);
                }

                Object.Scene = null;
                Object.Layer = null;
            }

            Recycle(this);
        }
    }
}
