using Molten.Collections;
using Molten.Graphics;
using Newtonsoft.Json;

namespace Molten
{
    public class ContentLoadBatch
    {
        public delegate void DispatchCompleteHandler(ContentLoadBatch loader);

        /// <summary>
        /// Invoked once the current <see cref="Dispatch"/> call has been completed.
        /// </summary>
        public event DispatchCompleteHandler OnCompleted;

        ThreadedList<ContentHandle> _handles;
        ThreadedList<ContentHandle> _toLoad;

        internal ContentLoadBatch(ContentManager manager)
        {
            Manager = manager;
            _handles = new ThreadedList<ContentHandle>();
            _toLoad = new ThreadedList<ContentHandle>();
        }

        /// <summary>
        /// Dispatches all <see cref="ContentHandle"/> that were set to load via the current <see cref="ContentLoadBatch"/>.
        /// </summary>
        public void Dispatch()
        {
            if (Status == ContentLoadBatchStatus.Dispatched)
                throw new InvalidOperationException("ContentLoadBatch has not yet completed the previous Dispatch()");


            _toLoad.Clear();
            _handles.ForEach((handle) =>
            {
                if (handle.Status == ContentHandleStatus.NotProcessed)
                    _toLoad.Add(handle);
            });

            if (Status == ContentLoadBatchStatus.NotDispatched &&
                _toLoad.Count == 0)
            {
                Status = ContentLoadBatchStatus.Completed;
                OnCompleted?.Invoke(this);
                return;
            }


            _handles.For(0, (index, handle) =>
            {
                if (handle.Asset != null)
                    _toLoad.Remove(handle);

                if (handle.Status != ContentHandleStatus.Processing)
                    handle.Dispatch();
            });
        }

        public ContentLoadHandle LoadFont(string path, 
            ContentLoadCallbackHandler<SpriteFont> completionCallback = null, 
            SpriteFontParameters parameters = null, 
            bool canHotReload = true)
        {
            if (Status == ContentLoadBatchStatus.Dispatched)
                throw new InvalidOperationException("Cannot load more content before Dispatch() is complete");

            ContentLoadHandle newHandle = Manager.LoadFont(path,
            (asset, isReload, handle) => AssetLoadCallback(asset, isReload, handle, completionCallback), 
            parameters, canHotReload, false);

            _handles.Add(newHandle);
            return newHandle;
        }

        public ContentLoadHandle Load<T>(string path, 
            ContentLoadCallbackHandler<T> completionCallback = null, 
            ContentParameters parameters = null, 
            bool canHotReload = true)
        {
            if (Status == ContentLoadBatchStatus.Dispatched)
                throw new InvalidOperationException("Cannot load more content before Dispatch() is complete");

            ContentLoadHandle newHandle = Manager.Load<T>(path, 
                (asset, isReload, handle) => AssetLoadCallback<T>(asset, isReload, handle, completionCallback),
                parameters, canHotReload, false);

            _handles.Add(newHandle);

            return newHandle;
        }

        public ContentLoadJsonHandle Deserialize<T>(string path, ContentLoadCallbackHandler<T> completionCallback = null, JsonSerializerSettings settings = null, bool canHotReload = true)
        {
            if (Status == ContentLoadBatchStatus.Dispatched)
                throw new InvalidOperationException("Cannot load more content before Dispatch() is complete");

            ContentLoadJsonHandle newHandle = Manager.Deserialize<T>(path, 
                (asset, isReload, handle) => AssetLoadCallback<T>(asset, isReload, handle, completionCallback), 
                settings, canHotReload, false);

            _handles.Add(newHandle);

            return newHandle;
        }

        private void AssetLoadCallback<T>(T asset, bool isReload, ContentHandle handle, ContentLoadCallbackHandler<T> completionCallback)
        {
            if (!isReload && _toLoad.Remove(handle))
            {
                if (_toLoad.Count == 0)
                {
                    Status = ContentLoadBatchStatus.Completed;
                    OnCompleted?.Invoke(this);
                }
            }

            completionCallback?.Invoke(asset, isReload, handle);
        }

        /// <summary>
        /// Gets the <see cref="ContentManager"/> that the current <see cref="ContentLoadBatch"/> is bound to.
        /// </summary>
        public ContentManager Manager { get; }

        /// <summary>
        /// Gets the total number of <see cref="ContentLoadHandle"/> 
        /// </summary>
        public int Count => _handles.Count;

        /// <summary>
        /// Gets the number of handles that have finished loading for the first time via the current <see cref="ContentLoadBatch"/>.
        /// </summary>
        public int LoadedCount => _toLoad.UnsafeCount;

        /// <summary>
        /// Gets the status of the current <see cref="ContentLoadBatch"/>.
        /// </summary>
        public ContentLoadBatchStatus Status { get; private set; }
    }
}
