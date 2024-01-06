using Molten.Collections;

namespace Molten.Threading;

internal class WorkerThread
{
    ThreadedQueue<WorkerTask> _queue;
    Thread _thread;
    bool _shouldExit;

    internal WorkerThread(string name, WorkerGroup grp, ThreadedQueue<WorkerTask> taskQueue)
    {
        Group = grp;
        _queue = taskQueue;

        _thread = new Thread(ProcessQueue);
        _thread.Name = name;

        try
        {
            _thread.TrySetApartmentState(grp.ThreadApartment);
        }
        finally { }
    }

    private void ProcessQueue()
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
                if (!_shouldExit && Group.Reset.Reset())
                    Group.Reset.WaitOne();
            }
        }
    }

    internal void Start()
    {
        _thread.Start();
    }

    internal void Exit()
    {
        _shouldExit = true;
    }

    internal void ExitAndJoin(TimeSpan? timeout = null)
    {
        Exit();

        if (timeout.HasValue)
            _thread.Join(timeout.Value);
        else
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
