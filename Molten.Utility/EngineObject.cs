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

        static JsonSerializerSettings _defaultJsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
        };

        /// <summary>Safely disposes of an object.</summary>
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
                throw new ObjectDisposedException("Object has already been disposed.");

            _isDisposed = true;
            OnDisposing?.Invoke(this);
            OnDispose();

            // TODO free object ID
        }

        /// <summary>Invoked when <see cref="Dispose"/> is called.</summary>
        protected virtual void OnDispose() { }

        /// <summary>Serializes the current instance into JSON and returns the result.</summary>
        /// <param name="formatting">The type of JSON formatting.</param>
        public virtual string ToJson(JsonSerializerSettings settings = null)
        {
            return JsonConvert.SerializeObject(this, settings ?? _defaultJsonSettings);
        }

        /// <summary>Deserializes a JSON string and populates the current instance with the result.</summary>
        /// <param name="json">The json to deserialize.</param>
        /// <param name="settings">Custom JSON settings to use when deserializing.</param>
        public virtual void FromJson(string json, JsonSerializerSettings settings = null)
        {
            JsonConvert.PopulateObject(json, this, settings ?? _defaultJsonSettings);
        }

        /// <summary>Gets whether or not the object has been disposed.</summary>
        public bool IsDisposed => _isDisposed;
    }
}
