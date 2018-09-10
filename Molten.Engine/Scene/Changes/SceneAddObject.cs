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

        public SceneLayer Layer;

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
                {
                    SceneLayer oldLayer = Object.Layer;
                    oldLayer.Objects.Remove(Object);

                    if (Object is IUpdatable oldUp)
                        oldLayer.Updatables.Add(oldUp);

                    if (Object is ICursorAcceptor oldAcceptor)
                        oldLayer.InputAcceptors.Add(oldAcceptor);

                    if (Object is IRenderable2D oldR2D)
                    {
                        oldLayer.Renderables2d.Add(oldR2D);
                        Object.Scene.RenderData.AddObject(oldR2D, oldLayer.Data);
                    }
                }

                Object.Scene = scene;
                Object.Layer = Layer;
                Layer.Objects.Add(Object);

                if (Object is IUpdatable up)
                    Layer.Updatables.Add(up);

                if (Object is ICursorAcceptor acceptor)
                    Layer.InputAcceptors.Add(acceptor);

                if (Object is IRenderable2D r2d)
                {
                    Layer.Renderables2d.Add(r2d);
                    scene.RenderData.AddObject(r2d, Layer.Data);
                }
            }

            Recycle(this);
        }
    }
}
