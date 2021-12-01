using Molten.Collections;
using Molten.Threading;

namespace Molten
{
    internal class ContentReloadTask : IWorkerTask, IPoolable
    {
        static ObjectPool<ContentReloadTask> _pool = new ObjectPool<ContentReloadTask>(() => new ContentReloadTask());
        internal static ContentReloadTask Get()
        {
            return _pool.GetInstance();
        }

        public event WorkerTaskCompletedEvent OnCompleted;
        internal ContentFile File;
        internal ContentManager Manager;

        public void ClearForPool()
        {
            File = null;
        }

        public void Run()
        {
            Manager.ReloadFile(File);
            OnCompleted?.Invoke(this);
            _pool.Recycle(this);
        }
    }
}
