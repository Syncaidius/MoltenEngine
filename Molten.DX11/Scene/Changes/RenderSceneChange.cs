using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal abstract class RenderSceneChange : IPoolable
    {
        public abstract void Clear();

        public abstract void Process(SceneRenderDataDX11 data);
    }

    internal abstract class RenderSceneChange<CHANGE> : RenderSceneChange
        where CHANGE : RenderSceneChange, new()
    {
        static ObjectPool<CHANGE> _pool = new ObjectPool<CHANGE>(() => new CHANGE());

        public static CHANGE Get()
        {
            return _pool.GetInstance();
        }

        protected static void Recycle(CHANGE obj)
        {
            _pool.Recycle(obj);
        }
    }
}
