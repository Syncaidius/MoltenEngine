using Molten.Collections;

namespace Molten.Graphics
{
    public abstract class RendererTask : IPoolable
    {
        public abstract void ClearForPool();

        public abstract void Process(RenderService renderer);
    }

    public abstract class RendererTask<T> : RendererTask
        where T : RendererTask, new()
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
