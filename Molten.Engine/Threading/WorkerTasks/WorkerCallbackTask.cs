namespace Molten.Threading
{
    /// <summary>
    /// A <see cref="WorkerTask"/> which executes <see cref="Callback"/> when run.
    /// </summary>
    public sealed class WorkerCallbackTask : WorkerTask
    {
        internal WorkerCallbackTask() { }

        protected override bool OnRun()
        {
            return Callback?.Invoke() ?? true;
        }

        protected override void OnFree() { }

        /// <summary>
        /// Gets or sets the callback method to be run by the current <see cref="WorkerCallbackTask"/>.
        /// </summary>
        public Func<bool> Callback { get; set; }
    }
}
