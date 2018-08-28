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

        public override void Clear()
        {
            Object = null;
        }

        internal override void Process(Scene scene)
        {
            if (Object.Scene == scene)
            {
                scene.Objects.Remove(Object);

                if(Object is IUpdatable up)
                    scene.Updatables.Remove(up);

                if (Object is ICursorAcceptor acceptor)
                    scene.InputAcceptors.Remove(acceptor);

                if (Object is IRenderable2D r2d)
                {
                    scene.Renderables2d.Remove(r2d);
                    scene.RenderData.RemoveObject(r2d);
                }

                Object.Scene = null;
            }

            Recycle(this);
        }
    }
}
