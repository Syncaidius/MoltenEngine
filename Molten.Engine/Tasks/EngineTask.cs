using Molten.Collections;

namespace Molten
{
    internal abstract class EngineTask : IPoolable
    {
        public abstract void Clear();

        public abstract void Process(Engine engine, Timing time);
    }

    internal abstract class EngineTask<T> : EngineTask
        where T : EngineTask, new()
    {
        static ObjectPool<T> _pool = new ObjectPool<T>(() => new T());

        public static T Get()
        {
            return _pool.GetInstance();
        }

        protected static void Recycle(T obj)
        {
            _pool.Recycle(obj);
        }
    }
}
