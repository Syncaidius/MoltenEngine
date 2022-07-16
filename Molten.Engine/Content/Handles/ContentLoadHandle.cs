using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public abstract class ContentLoadHandle<T> : ParameterizedContentHandle
    {
        internal Action<T> _completionCallback;

        bool _canHotReload;
        ContentWatcher _watcher;

        internal ContentLoadHandle(
            ContentManager manager, 
            IContentProcessor processor, 
            IContentParameters parameters, 
            Action<T> completionCallback, 
            bool canHotReload = true) : 
            base(manager, processor, parameters, typeof(T))
        {
            _completionCallback = completionCallback;
            _canHotReload = canHotReload;
            Asset = default(T);
        }

        protected override void OnComplete()
        {
            _completionCallback?.Invoke((T)Asset);

            // Setup a watcher for the file so we can reload it if any changes occur.
            if(_watcher == null && _canHotReload)
                _watcher = Manager.StartWatching(this);
        }

        protected override bool OnProcess()
        {
            if (Processor.Read(this, Asset, out Asset))
            {
                Manager.Log.WriteLine($"[CONTENT] [LOAD] {Path}: {Asset.GetType().FullName}");
                return base.OnProcess();
            }

            return false;
        }


        /// <summary>
        /// Gets the asset held by the current <see cref="ContentLoadHandle{T}"/>.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">If the asset has not been loaded yet.</exception>
        public T Get()
        {
            if (Status != ContentHandleStatus.Completed)
                throw new ContentNotLoadedException("Unable to retrieve asset that has not loaded yet.");

            return (T)Asset;
        }

        /// <summary>
        /// Gets or sets whether the asset is allowed to hot-reload when changes occur to it's file.
        /// </summary>
        public bool CanHotReload
        {
            get => _canHotReload;
            set
            {
                if (_canHotReload != value)
                {
                    _canHotReload = value;

                    // Delete the watcher if we have one to prevent reloads.
                    if (!_canHotReload && _watcher != null)
                    {
                        Manager.StopWatching(this);
                    }
                }
            }
        }
    }
}
