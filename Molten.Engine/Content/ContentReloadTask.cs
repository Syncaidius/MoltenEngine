using Molten.Collections;
using Molten.Threading;

namespace Molten
{
    internal class ContentReloadTask : WorkerTask, IPoolable
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

        protected override bool OnRun()
        {
            Manager.ReloadFile(File);
            OnCompleted?.Invoke(this);
            _pool.Recycle(this);
            return true;
        }
    }
}
