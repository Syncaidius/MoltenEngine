using System;
using System.Runtime.Serialization;
using System.Threading;

namespace Molten
{
    public delegate void EngineObjectHandler(EngineObject obj);

    /// <summary>
    /// A helper base class for tracking and managing game objects. Provides a disposal structure and unique ID system.
    /// </summary>
    [DataContract]
    public abstract class EngineObject : IDisposable
    {
        [ThreadStatic]
        static uint _idCounter;

        ulong _id;

        /// <summary>
        /// Invoked when the current <see cref="EngineObject"/> is being disposed.
        /// </summary>
        public event EngineObjectHandler OnDisposing;

        /// <summary>
        /// Creates a new instance of <see cref="EngineObject"/>.
        /// </summary>
        public EngineObject()
        {
            _id = ((ulong)Thread.CurrentThread.ManagedThreadId << 32) | _idCounter++;
            Name = $"EO {_id}";
        }

        /// <summary>Safely disposes of an object which may also be null.</summary>
        /// <param name="disposable">The object to dispose.</param>
        protected void DisposeObject<T>(ref T disposable) 
            where T : IDisposable
        {
            if (disposable != null)
            {
                disposable.Dispose();
                disposable = default;
            }
        }

        /// <summary>Disposes of the current <see cref="EngineObject"/> instance and 
        /// releases its ID to be reused by a new object.</summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            Interlocked.CompareExchange(ref OnDisposing, null, null)?.Invoke(this);
            OnDispose();
        }

        /// <summary>Invoked when <see cref="Dispose"/> is called.</summary>
        protected abstract void OnDispose();

        /// <summary>Gets whether or not the object has been disposed.</summary>
        public bool IsDisposed { get; protected set; }

        /// <summary>
        /// Gets or sets the tag object.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gets the unique <see cref="EngineObject"/> ID (EOID) of the current <see cref="EngineObject"/>.
        /// </summary>
        public ulong EOID => _id;

        /// <summary>
        /// Gets the name of the object. Multiple <see cref="EngineObject"/> can have the same name.
        /// </summary>
        public string Name { get; protected set; }
    }
}
