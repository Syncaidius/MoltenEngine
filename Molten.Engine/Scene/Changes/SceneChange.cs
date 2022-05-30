using Molten.Collections;

namespace Molten
{
    internal abstract class SceneChange : IPoolable
    {
        public Scene Scene { get; internal set; }

        public abstract void ClearForPool();

        internal abstract void Process();
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
