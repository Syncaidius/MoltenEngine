using Molten.Collections;
using Molten.Utility.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Molten.Threading
{
    /// <summary>A group of worker threads that process tasks stored on a shared queue. This is great for chewing through a queue full of one-time tasks which don't need to run
    /// inside an engine loop, such as AI pathfinding tasks or AI logic, asset loading, etc.</summary>
    public class WorkerGroup : IDisposable
    {
        ThreadedQueue<IWorkerTask> _queue;
        List<WorkerThread> _threads;
        ThreadManager _manager;
        string _name;
        int _nameCount;
        internal WorkerGroup(ThreadManager manager, string name, int workerCount, ThreadedQueue<IWorkerTask> workQueue = null)
        {
            _queue = workQueue ?? new ThreadedQueue<IWorkerTask>();
            _threads = new List<WorkerThread>();
            _name = name;
            _manager = manager;
            SetWorkerCount(workerCount);
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            for (int i = 0; i < _threads.Count; i++)
                _threads[i].Exit();

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

        public void QueueTask(IWorkerTask task)
        {
            _queue.Enqueue(task);
            for (int i = 0; i < _threads.Count; i++)
                _threads[i].Wake();
        }

        public void QueueTasks(IEnumerable<IWorkerTask> tasks)
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
                while(_threads.Count > count)
                {
                    WorkerThread t = _threads[0];
                    t.Exit();
                    _threads.Remove(t);
                }
            }
            else if (_threads.Count < count)
            {
                while(_threads.Count < count)
                {
                    string tName = $"{_name}_{_nameCount}";
                    WorkerThread t = new WorkerThread(_manager.Engine, tName, _queue);
                    _threads.Add(t);
                    t.Start();
                    _nameCount++;
                }
            }
        }

        /// <summary>Set the worker's task queue.</summary>
        public void SetTaskQueue(ThreadedQueue<IWorkerTask> taskQueue)
        {
            _queue = taskQueue;
        }

        /// <summary>Gets the number of threads in the worker group.</summary>
        public int WorkerCount => _threads.Count;

        public string Name => _name;

        public bool IsDisposed { get; internal set; }
    }
}
