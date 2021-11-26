namespace Molten.Threading
{
    public delegate void WorkerTaskCompletedEvent(IWorkerTask task);

    public interface IWorkerTask
    {
        /// <summary>Invoked when a <see cref="WorkerThread"/> completes a task.</summary>
        event WorkerTaskCompletedEvent OnCompleted;

        void Run();
    }
}
