namespace Molten.Threading;

public delegate void WorkerTaskCompletedEvent(WorkerTask task);

public abstract class WorkerTask : IDisposable
{
    /// <summary>Invoked when a <see cref="WorkerThread"/> completes a task.</summary>
    public event WorkerTaskCompletedEvent OnCompleted;

    ManualResetEvent _waitHandle = new ManualResetEvent(false);

    internal bool Run()
    {
        _waitHandle.Reset();

        if (OnRun())
        {
            OnCompleted?.Invoke(this);
            OnFree();
            _waitHandle.Set();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Invoked after the current <see cref="WorkerTask"/> is completed and about to be released from a <see cref="WorkerThread"/>.
    /// </summary>
    protected abstract void OnFree();

    /// <summary>
    /// Blocks the calling <see cref="Thread"/> until the current <see cref="WorkerTask"/> is completed.
    /// </summary>
    public void Wait()
    {
        _waitHandle.WaitOne();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected abstract bool OnRun();

    void IDisposable.Dispose()
    {
        _waitHandle.Set();
        _waitHandle.Dispose();
    }
}
