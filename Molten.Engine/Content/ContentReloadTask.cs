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

        internal ContentFile File;
        internal ContentManager Manager;

        public void ClearForPool()
        {
            File = null;
        }

        protected override bool OnRun()
        {
            Manager.ReloadFile(File);
            return true;
        }

        protected override void OnFree()
        {
            _pool.Recycle(this);
        }
    }
}
