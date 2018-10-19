using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Molten
{
    public delegate void EngineObjectHandler(EngineObject obj);

    [DataContract]
    public abstract class EngineObject : IDisposable
    {
        bool _isDisposed;

        public event EngineObjectHandler OnDisposing;

        /// <summary>Safely disposes of an object which may also be null.</summary>
        /// <param name="disposable">The object to dispose.</param>
        protected void DisposeObject<T>(ref T disposable) where T : IDisposable
        {
            if (disposable != null)
            {
                disposable.Dispose();
                disposable = default;
            }
        }

        /// <summary>Disposes of the current <see cref="EngineObject"/> instance and releases its ID to be reused by a new object.</summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            Interlocked.CompareExchange(ref OnDisposing, null, null)?.Invoke(this);
            OnDispose();
        }

        /// <summary>Invoked when <see cref="Dispose"/> is called.</summary>
        protected virtual void OnDispose() { }

        /// <summary>Gets whether or not the object has been disposed.</summary>
        public bool IsDisposed => _isDisposed;

        /// <summary>
        /// Gets or sets the tag object.
        /// </summary>
        public object Tag { get; set; }
    }
}
