using Molten.Collections;

namespace Molten.Graphics
{
    public abstract class RenderSceneChange : IPoolable
    {
        public abstract void Clear();

        public abstract void Process();
    }

    public abstract class RenderSceneChange<CHANGE> : RenderSceneChange
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
