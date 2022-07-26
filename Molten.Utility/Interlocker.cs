namespace Molten
{
    /// <summary>
    /// A locking helper built on top of the <see cref="Interlocked"/> class.
    /// </summary>
    public class Interlocker
    {
        int _blockingVal;

        /// <summary>
        /// Gets the <see cref="Thread"/> which locked the current <see cref="Interlocker"/>.
        /// </summary>
        public Thread LockingThread { get; private set; }

        /// <summary>
        /// Takes the lock by internally using <see cref="Interlocked.Exchange(ref int, int)"/>. Skips lock checks if the current thread already holds the lock.
        /// </summary>
        /// <param name="callback">The callback to invoke once a lock is acquired.</param>
        public void Lock(Action callback)
        {
            // Ignore locking if the current thread already own's the lock.
            // This allows nested locks on the same thread.
            if (LockingThread != Thread.CurrentThread)
            {
                SpinWait spin = new SpinWait();
                while (0 != Interlocked.Exchange(ref _blockingVal, 1))
                    spin.SpinOnce();

                LockingThread = Thread.CurrentThread;
                callback();
                LockingThread = null;
                Interlocked.Exchange(ref _blockingVal, 0);
            }
            else
            {
                callback();
            }
        }

        /// <summary>
        /// Releases the lock then throws an exception.
        /// </summary>
        /// <typeparam name="T">The type of exception. Note: The exception type must have a constructor which accepts only a string for the message.</typeparam>
        /// <param name="message">The message to attach to the exception.</param>
        public void Throw<T>(string message) where T : Exception
        {
            T ex = Activator.CreateInstance(typeof(T), message) as T;
            LockingThread = null;
            Interlocked.Exchange(ref _blockingVal, 0);
            throw ex;
        }

        /// <summary>
        /// Releases the lock then throws an exception.
        /// </summary>
        /// <param name="exception">The exception to be thrown.</param>
        public void Throw(Exception exception)
        {
            LockingThread = null;
            Interlocked.Exchange(ref _blockingVal, 0);
            throw exception;
        }
    }
}
