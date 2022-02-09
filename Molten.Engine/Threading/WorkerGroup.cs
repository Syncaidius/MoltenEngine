using Molten.Collections;
using System;
using System.Collections.Generic;

namespace Molten.Threading
{
    /// <summary>A group of worker threads that process tasks stored on a shared queue. This is great for chewing through a queue full of one-time tasks which don't need to run
    /// inside an engine loop, such as AI pathfinding tasks or AI logic, asset loading, etc.</summary>
    public class WorkerGroup : IDisposable
    {
        ThreadedQueue<WorkerTask> _queue;
        List<WorkerThread> _threads;
        ThreadManager _manager;
        string _name;
        int _nameCount;
        ApartmentState _apartment;

        /// <summary>
        /// Creates a new instance of <see cref="WorkerGroup"/>.
        /// </summary>
        /// <param name="manager">The <see cref="ThreadManager"/> which owns the group.</param>
        /// <param name="name">The name of the group.</param>
        /// <param name="workerCount">The number of <see cref="WorkerThread"/> in the group.</param>
        /// <param name="workQueue">A queue from which the worker threads will acquire tasks. If null, a queue will be created internally.</param>
        /// <param name="apartment"></param>
        internal WorkerGroup(ThreadManager manager, string name, int workerCount, ThreadedQueue<WorkerTask> workQueue = null, ApartmentState apartment = ApartmentState.MTA)
        {
            _queue = workQueue ?? new ThreadedQueue<WorkerTask>();
            _threads = new List<WorkerThread>();
            _name = name;
            _manager = manager;
            _apartment = apartment;

            SetWorkerCount(workerCount);
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            for (int i = 0; i < _threads.Count; i++)
                _threads[i].ExitAndJoin();

            _threads.Clear();

            _manager.DestroyGroup(this);
            IsDisposed = true;
        }

        /// <summary>Exits all worker threads and empties the task queue.</summary>
        public void Clear()
        {
            for (int i = 0; i < _threads.Count; i++)
                _threads[i].Exit();

            _threads.Clear();
            _queue.Clear();
        }

        public void QueueTask(WorkerTask task)
        {
            _queue.Enqueue(task);
            for (int i = 0; i < _threads.Count; i++)
                _threads[i].Wake();
        }

        /// <summary>
        /// Queues a callback method for execution by the current <see cref="WorkerGroup"/>.
        /// </summary>
        /// <param name="callback">The callback to be executed. Must return a <see cref="bool"/>.</param>
        /// <remarks> Returning true signifies that the task is completed. 
        /// Returning false will cause the task to be re-queued until newer jobs have been completed.</remarks>
        /// <returns></returns>
        public WorkerCallbackTask QueueCallback(Func<bool> callback)
        {
            WorkerCallbackTask task = new WorkerCallbackTask();
            task.Callback = callback;
            QueueTask(task);
            return task;
        }

        public void QueueTasks(IEnumerable<WorkerTask> tasks)
        {
            _queue.EnqueueRange(tasks);
            for (int i = 0; i < _threads.Count; i++)
                _threads[i].Wake();
        }

        public void SetWorkerCount(int count)
        {
            if (IsDisposed)
                return;

            if (_threads.Count > count)
            {
                while (_threads.Count > count)
                {
                    WorkerThread t = _threads[0];
                    t.Exit();
                    _threads.Remove(t);
                }
            }
            else if (_threads.Count < count)
            {
                while (_threads.Count < count)
                {
                    string tName = $"{_name}_{_nameCount}";
                    WorkerThread t = new WorkerThread(tName, this, _queue, _apartment);
                    _threads.Add(t);
                    t.Start();
                    _nameCount++;
                }
            }
        }

        /// <summary>Gets the number of threads in the worker group.</summary>
        public int WorkerCount => _threads.Count;

        /// <summary>
        /// Gets the name of the current <see cref="WorkerGroup"/>.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Gets whether or not the current <see cref="WorkerGroup"/> has been disposed.
        /// </summary>
        public bool IsDisposed { get; internal set; }
    }
}
