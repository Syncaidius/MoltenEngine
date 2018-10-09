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
                if (Object.Layer != null)
                    Object.Layer.Objects.Remove(Object);

                Object.Layer = Layer;
                Layer.Objects.Add(Object);
            }

            Recycle(this);
        }
    }
}
