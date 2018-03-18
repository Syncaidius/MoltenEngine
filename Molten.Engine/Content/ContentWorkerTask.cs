using Molten.Collections;
using Molten.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    internal class ContentWorkerTask : IWorkerTask, IPoolable
    {
        static ObjectPool<ContentWorkerTask> _pool = new ObjectPool<ContentWorkerTask>(() => new ContentWorkerTask());
        internal static ContentWorkerTask Get()
        {
            return _pool.GetInstance();
        }

        public event WorkerTaskCompletedEvent OnCompleted;
        internal ContentRequest Request;

        public void Clear()
        {
            Request = null;
        }

        public void Run(Engine engine)
        {
            Request.Manager.ProcessRequest(Request);
            OnCompleted?.Invoke(this);
            _pool.Recycle(this);
        }
    }
}
