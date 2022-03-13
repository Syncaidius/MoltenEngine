using Molten.Collections;
using System;
using System.Threading;

namespace Molten.Threading
{
    internal class WorkerThread
    {
        ThreadedQueue<WorkerTask> _queue;
        AutoResetEvent _reset;
        Thread _thread;
        bool _shouldExit;

        internal WorkerThread(string name, WorkerGroup grp, ThreadedQueue<WorkerTask> taskQueue)
        {
            Group = grp;
            _reset = new AutoResetEvent(false);
            _queue = taskQueue;

            _thread = new Thread(() =>
            {
                WorkerTask task = null;

                while (!_shouldExit)
                {
                    if (!Group.IsPaused && _queue.TryDequeue(out task))
                    {
                        // If the task did not complete, put it back on the queue.
                        if (!task.Run())
                            Group.QueueTask(task);

                        task = null;
                    }
                    else
                    {
                        _reset.WaitOne();
                    }
                }
            });

            _thread.Name = name;

            try
            {
                _thread.TrySetApartmentState(grp.ThreadApartment);
            }
            finally { }
        }

        internal void Wake()
        {
            _reset.Set();
        }

        internal void Start()
        {
            _thread.Start();
        }

        internal void Exit()
        {
            _shouldExit = true;
            Wake();
        }

        internal void ExitAndJoin()
        {
            Exit();
            _thread.Join();
        }


        /// <summary>
        /// Gets the name of the thread.
        /// </summary>
        public string Name => _thread.Name;

        /// <summary>
        /// Gets the <see cref="WorkerGroup"/> that the current <see cref="WorkerThread"/> belongs to.
        /// </summary>
        public WorkerGroup Group { get; }
    }
}
