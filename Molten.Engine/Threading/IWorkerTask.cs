using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Threading
{
    public delegate void WorkerTaskCompletedEvent(IWorkerTask task);

    public interface IWorkerTask
    {
        /// <summary>Invoked when a <see cref="WorkerThread"/> completes a task.</summary>
        event WorkerTaskCompletedEvent OnCompleted;

        void Run(Engine engine);
    }
}
