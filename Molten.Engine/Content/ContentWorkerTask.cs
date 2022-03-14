using Molten.Collections;
using Molten.Threading;

namespace Molten
{
    internal class ContentWorkerTask : WorkerTask, IPoolable
    {
        static ObjectPool<ContentWorkerTask> _pool = new ObjectPool<ContentWorkerTask>(() => new ContentWorkerTask());
        internal static ContentWorkerTask Get()
        {
            return _pool.GetInstance();
        }

        public event WorkerTaskCompletedEvent OnCompleted;
        internal ContentRequest Request;

        public void ClearForPool()
        {
            Request = null;
        }

        protected override bool OnRun()
        {
            Request.Manager.ProcessRequest(Request);
            return true;
        }

        protected override void OnFree()
        {
            _pool.Recycle(this);
        }
    }
}
