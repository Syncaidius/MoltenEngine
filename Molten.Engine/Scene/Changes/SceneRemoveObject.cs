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
        internal SceneObject Object;

        internal SceneLayer Layer;

        public override void Clear()
        {
            Object = null;
            Layer = null;
        }

        internal override void Process(Scene scene)
        {
            if (Object.Layer == Layer)
            {
                Layer.Objects.Remove(Object);
                Object.Layer = null;
            }

            Recycle(this);
        }
    }
}
