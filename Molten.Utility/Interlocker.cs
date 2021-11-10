using System;
using System.Threading;

namespace Molten
{
    /// <summary>
    /// A locking helper built on top of the <see cref="Interlocked"/> class.
    /// </summary>
    public class Interlocker
    {
        Thread _thread;
        int _blockingVal;

        /// <summary>
        /// Takes the lock by internally using <see cref="Interlocked.Exchange(ref int, int)"/>. Skips lock checks if the current thread already holds the lock.
        /// </summary>
        /// <param name="callback"></param>
        public void Lock(Action callback)
        {
            if (_thread != Thread.CurrentThread)
            {
                SpinWait spin = new SpinWait();
                while (0 != Interlocked.Exchange(ref _blockingVal, 1))
                    spin.SpinOnce();

                _thread = Thread.CurrentThread;
                callback();
                _thread = null;
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
            _thread = null;
            Interlocked.Exchange(ref _blockingVal, 0);
            throw ex;
        }

        /// <summary>
        /// Releases the lock then throws an exception.
        /// </summary>
        /// <param name="exception">The exception to be thrown.</param>
        public void Throw(Exception exception)
        {
            _thread = null;
            Interlocked.Exchange(ref _blockingVal, 0);
            throw exception;
        }
    }
}
