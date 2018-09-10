using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class SceneException : Exception
    {
        public Scene Scene { get; private set; }

        public SceneException(Scene scene, string message) : base(message)
        {
            Scene = scene;
        }
    }

    public class SceneObjectException : SceneException
    {
        public SceneObject Object { get; private set; }

        public SceneObjectException(Scene scene, SceneObject obj, string message) : base(scene, message)
        {
            Object = obj;
        }
    }

    public class SceneLayerException : SceneException
    {
        public SceneLayer Layer { get; private set; }

        public SceneLayerException(Scene scene, SceneLayer layer, string message) : base(scene, message)
        {
            Layer = layer;
        }
    }
}
