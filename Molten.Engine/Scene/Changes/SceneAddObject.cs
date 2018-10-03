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
                }

                Object.Layer = Layer;
                Layer.Objects.Add(Object);

                if (Object is IUpdatable up)
                    Layer.Updatables.Add(up);

                if (Object is ICursorAcceptor acceptor)
                    Layer.InputAcceptors.Add(acceptor);
            }

            Recycle(this);
        }
    }
}
