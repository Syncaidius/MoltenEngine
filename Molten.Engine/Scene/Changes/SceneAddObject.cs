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
        public SceneObject Object;

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
                scene.Updatables.Add(Object);
            }

            Recycle(this);
        }
    }
}
