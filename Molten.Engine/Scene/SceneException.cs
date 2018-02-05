using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class SceneException : Exception
    {
        public SceneObject Object { get; private set; }

        public Scene Scene { get; private set; }

        public SceneException(Scene scene, SceneObject obj, string message) : base(message) { }
    }
}
