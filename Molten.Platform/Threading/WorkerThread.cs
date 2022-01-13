using Molten.Collections;
using System;
using System.Threading;

namespace Molten.Threading
{
    internal class WorkerThread
    {
        ThreadedQueue<IWorkerTask> _queue;
        AutoResetEvent _reset;
        Thread _thread;
        bool _shouldExit;

        internal WorkerThread(string name, ThreadedQueue<IWorkerTask> taskQueue, ApartmentState apartment)
        {
            Apartment = apartment;
            _reset = new AutoResetEvent(false);
            _queue = taskQueue;

            _thread = new Thread(() =>
            {
                IWorkerTask task = null;

                while (!_shouldExit)
                {
                    if (_queue != null && _queue.TryDequeue(out task))
                        task.Run();
                    else
                        _reset.WaitOne();
                }
            });

            _thread.Name = name;
            try
            {
                _thread.TrySetApartmentState(apartment);
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

        internal void Abort()
        {
            _thread.Abort();
        }

        internal void Exit()
        {
            _shouldExit = true;
            Wake();
        }


        /// <summary>
        /// Gets the name of the thread.
        /// </summary>
        public string Name => _thread.Name;

        /// <summary>
        /// Gets the <see cref="ApartmentState"/> of the current thread.
        /// </summary>
        public ApartmentState Apartment { get; }
    }
}
