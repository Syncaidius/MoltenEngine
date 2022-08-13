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
        /// Acquires an interlocked-based lock. Subsequent calls to <see cref="Lock"/> from the same thread will be ignored until <see cref="Unlock"/> is called.
        /// </summary>
        public void Lock()
        {
            // Ignore locking if the current thread already own's the lock.
            // This allows nested locks on the same thread.
            if (LockingThread == Thread.CurrentThread)
                return;

            SpinWait spin = new SpinWait();
            while (0 != Interlocked.Exchange(ref _blockingVal, 1))
                spin.SpinOnce();

            LockingThread = Thread.CurrentThread;
        }

        /// <summary>
        /// Releases the previously' acquired lock. Can only be performed by the thread which acquired <see cref="Lock"/>.
        /// </summary>
        public void Unlock()
        {
            if (LockingThread != Thread.CurrentThread)
                throw new ThreadStateException($"The thread calling Unlock() is not the one holding the current Lock()");

            LockingThread = null;
            Interlocked.Exchange(ref _blockingVal, 0);
        }

        /// <summary>
        /// Takes the lock by internally using <see cref="Lock"/> and <see cref="Unlock"/> 
        /// </summary>
        /// <param name="callback">The callback to invoke once a lock is acquired.</param>
        public void Lock(Action callback)
        {
            Lock();
            callback();
            Unlock();
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
