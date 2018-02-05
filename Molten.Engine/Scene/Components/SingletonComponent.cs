using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public sealed class SingletonComponent<T> : SceneComponent where T : SceneSingleton, new()
    {
        static T _singleton;

        public SingletonComponent()
        {
            _singleton = _singleton ?? new T();
        }

        public override void OnUpdate(Timing time)
        {
            _singleton.OnUpdate(Object, time);
        }
    }
}
