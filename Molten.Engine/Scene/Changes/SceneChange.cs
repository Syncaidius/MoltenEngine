using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    internal abstract class SceneChange : IPoolable
    {
        public abstract void Clear();

        internal abstract void Process(Scene scene);
    }

    internal abstract class SceneChange<CHANGE> : SceneChange
        where CHANGE : SceneChange, new()
    {
        static ObjectPool<CHANGE> _pool = new ObjectPool<CHANGE>(() => new CHANGE());

        internal static CHANGE Get()
        {
            return _pool.GetInstance();
        }

        internal static void Recycle(CHANGE obj)
        {
            _pool.Recycle(obj);
        }
    }
}
