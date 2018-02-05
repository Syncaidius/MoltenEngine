using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    /// <summary>A singleton object which is used in conjunction with <see cref="SingletonComponent{T}"/> to allow the application of a single behaviour on multiple <see cref="SceneObject"/>.</summary>
    public abstract class SceneSingleton
    {
        public abstract void OnAttach(SceneObject obj);

        public abstract void OnDetach(SceneObject obj);

        public abstract void OnUpdate(SceneObject obj, Timing time);
    }
}
