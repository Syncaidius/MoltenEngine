using Molten.Collections;
using System;
using System.Threading;

namespace Molten.Threading
{
    public class ThreadManager : IDisposable
    {
        Logger _log;
        ThreadedDictionary<string, EngineThread> _threadsByName;
        ThreadedDictionary<string, WorkerGroup> _groupsByName;
        ThreadedList<EngineThread> _threads;
        ThreadedList<WorkerGroup> _groups;

        public ThreadManager(Logger log)
        {
            _log = log;
            _threads = new ThreadedList<EngineThread>();
            _groups = new ThreadedList<WorkerGroup>();
            _threadsByName = new ThreadedDictionary<string, EngineThread>();
            _groupsByName = new ThreadedDictionary<string, WorkerGroup>();
        }

        /// <summary>Spawns a new <see cref="EngineThread"/> to run the provided callback."</summary>
        /// <param name="callback">The callback to be run by the thread.</param>
        /// <param name="apartmentState">The apartment state of the thread. The default value is multithreaded apartment (MTA).</param>
        /// <param name="fixedTimeStep">If true, the new thread will be set to use a fixed time-step.</param>
        /// <param name="name">The name of the new thread.</param>
        /// <param name="startImmediately">If true, the new thread will be started immediately after spawning.</param>
        public EngineThread SpawnThread(string name, bool startImmediately, bool fixedTimeStep, Action<Timing> callback, ApartmentState apartmentState = ApartmentState.MTA)
        {
            if (_threadsByName.ContainsKey(name))
                throw new EngineThreadException(null, name, "An engine thread with the same name already exists!");

            EngineThread thread = new EngineThread(this, name, startImmediately, fixedTimeStep, callback, apartmentState);

            _threads.Add(thread);
            _threadsByName.Add(name, thread);
            _log.WriteLine($"Spawned engine thread '{name}'");
            return thread;
        }

        public WorkerGroup SpawnWorkerGroup(string name, int workerCount, ThreadedQueue<IWorkerTask> workerQueue = null)
        {
            if (_groupsByName.ContainsKey(name))
                throw new EngineThreadException(null, name, "A worker group with the same name already exists!");

            WorkerGroup group = new WorkerGroup(this, name, workerCount, workerQueue);
            _groups.Add(group);
            _groupsByName.Add(name, group);
            _log.WriteLine($"Spawned worker group '{group.Name}");
            return group;
        }

        /// <summary>Attempts to retrieve a thread that was previously spawned by the thread manager.</summary>
        public bool TryGetThread(string name, out EngineThread thread)
        {
            return _threadsByName.TryGetValue(name, out thread);
        }

        public void DestroyThread(EngineThread thread)
        {
            if (thread.IsDisposed)
                return;

            _log.WriteLine($"Destroyed thread '{thread.Name}'");
            thread.IsDisposed = true;
            _threads.Remove(thread);
            _threadsByName.Remove(thread.Name);
        }

        public void DestroyGroup(WorkerGroup group)
        {
            if (group.IsDisposed)
                return;

            group.Clear();
            group.IsDisposed = true;
            _groups.Remove(group);
            _groupsByName.Remove(group.Name);
            _log.WriteLine($"Destroyed worker group '{group.Name}");
        }

        /// <summary>Exits all engine threads before disposing of the thread manager. Do not call this externally.</summary>
        public void Dispose()
        {
            for (int i = _threads.Count - 1; i >= 0; i--)
                _threads[i].DisposeAndJoin();
        }
    }
}
