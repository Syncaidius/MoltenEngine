using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Collections;
using Newtonsoft.Json;

namespace Molten
{
    public class ContentLoadBatch
    {
        ThreadedList<ContentHandle> _handles;

        volatile int _loadedCount;

        internal ContentLoadBatch(ContentManager manager)
        {
            Manager = manager;
            _handles = new ThreadedList<ContentHandle>();
        }

        /// <summary>
        /// Dispatches all <see cref="ContentHandle"/> that were set to load via the current <see cref="ContentLoadBatch"/>.
        /// </summary>
        public void Dispatch()
        {
            _handles.For(0, 1, (index, handle) =>
            {
                if(handle.Status != ContentHandleStatus.Processing)
                    handle.Dispatch();
            });
        }

        public ContentLoadHandle<T> Load<T>(string path, ContentLoadCallbackHandler<T> completionCallback = null, IContentParameters parameters = null, bool canHotReload = true)
        {
            ContentLoadHandle<T> handle = Manager.Load<T>(path, (asset, isReload) =>
            {
                if (!isReload)
                    _loadedCount++;

                completionCallback?.Invoke(asset, isReload);
            }, parameters, canHotReload, false);

            _handles.Add(handle);

            return handle;
        }

        public ContentLoadJsonHandle<T> Deserialize<T>(string path, ContentLoadCallbackHandler<T> completionCallback = null, JsonSerializerSettings settings = null, bool canHotReload = true)
        {
            ContentLoadJsonHandle<T> handle = Manager.Deserialize<T>(path, (asset, isReload) =>
            {
                if (!isReload)
                    _loadedCount++;

                completionCallback?.Invoke(asset, isReload);
            }, settings, canHotReload, false);

            _handles.Add(handle);

            return handle;
        }

        public ContentManager Manager { get; }

        /// <summary>
        /// Gets the total number of <see cref="ContentLoadHandle{T}="/> 
        /// </summary>
        public int Count => _handles.Count;

        /// <summary>
        /// Gets the number of handles that have finished loading for the first time via the current <see cref="ContentLoadBatch"/>.
        /// </summary>
        public int LoadedHandleCount => _loadedCount;
    }
}
