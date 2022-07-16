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
        internal T Asset;

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
            _completionCallback?.Invoke(Asset);

            // Setup a watcher for the file so we can reload it if any changes occur.
            if(_watcher == null && _canHotReload)
                _watcher = Manager.StartWatching(this);
        }

        protected override bool OnProcess()
        {
            return base.OnProcess() && Processor.Read(this, out Asset);
        }

        public T Get()
        {
            return Asset;
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
