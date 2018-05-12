using Molten.Collections;
using Molten.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void Clear()
        {
            File = null;
        }

        public void Run(Engine engine)
        {
            Manager.ReloadFile(File);
            OnCompleted?.Invoke(this);
            _pool.Recycle(this);
        }
    }
}
