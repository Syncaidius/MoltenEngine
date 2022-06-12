using Molten.Utility;
using System.Runtime.Serialization;

namespace Molten
{
    /// <summary>
    /// A helper base class for tracking and managing game objects. Provides a disposal structure and unique ID system.
    /// </summary>
    [DataContract]
    public abstract class EngineObject : IDisposable
    {
        [ThreadStatic]
        static uint _idCounter;

        /// <summary>
        /// Invoked when the current <see cref="EngineObject"/> is being disposed.
        /// </summary>
        public event MoltenEventHandler<EngineObject> OnDisposing;

        /// <summary>
        /// Creates a new instance of <see cref="EngineObject"/>.
        /// </summary>
        public EngineObject()
        {
            EOID = ((ulong)Thread.CurrentThread.ManagedThreadId << 32) | _idCounter++;
            Name = $"EO {EOID} - {this.GetType().Name}";
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>Disposes of the current <see cref="EngineObject"/> instance.</summary>
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
        /// Gets the unique <see cref="EngineObject"/> ID (EOID) of the current <see cref="EngineObject"/>.
        /// </summary>
        public ulong EOID { get; }

        /// <summary>
        /// Gets the name of the object. Multiple <see cref="EngineObject"/> can have the same name.
        /// </summary>
        public string Name { get; set; }
    }
}
