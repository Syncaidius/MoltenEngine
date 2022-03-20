using Molten.Threading;

namespace Molten.Input
{
    public class WindowsClipboard : IClipboard
    {
        WorkerGroup _workers;

        internal WindowsClipboard() { }

        internal void Start(ThreadManager thread)
        {
            _workers = thread.CreateWorkerGroup("Clipboard", 1);
        }

        internal void Stop()
        {
            _workers?.Dispose();
            _workers = null;
        }

        public void SetText(string txt)
        {
            WorkerCallbackTask task = _workers?.QueueCallback(() =>
            {
                if (!string.IsNullOrWhiteSpace(txt))
                    Clipboard.SetText(txt);

                return true;
            });

            task.Wait();
        }

        public bool ContainsText()
        {
            bool result = false;
            WorkerCallbackTask task = _workers?.QueueCallback(() =>
            {
                result = Clipboard.ContainsText();
                return true;
            });

            task.Wait();
            return result;
        }

        public string GetText()
        {
            string result = null;
            WorkerCallbackTask task = _workers?.QueueCallback(() =>
            {
                result = Clipboard.GetText();
                return true;
            });

            task.Wait();
            return result;
        }

        public void Dispose()
        {
            _workers?.Dispose();
        }
    }
}
