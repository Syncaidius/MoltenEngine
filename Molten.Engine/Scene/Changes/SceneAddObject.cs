using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    internal class SceneAddObject : SceneChange<SceneAddObject>
    {
        public ISceneObject Object;

        public override void Clear()
        {
            Object = null;
        }

        internal override void Process(Scene scene)
        {
            if (Object.Scene != scene)
            {
                // Remove from other scene
                if (Object.Scene != null)
                    Object.Scene.Objects.Remove(Object);

                Object.Scene = scene;
                scene.Objects.Add(Object);

                if (Object is IUpdatable up)
                    scene.Updatables.Add(up);

                if (Object is IInputAcceptor acceptor)
                    scene.InputAcceptors.Add(acceptor);

                if (Object is IRenderable2D r2d)
                {
                    scene.Renderables2d.Add(r2d);
                    scene.RenderData.AddObject(r2d);
                }
            }

            Recycle(this);
        }
    }
}
