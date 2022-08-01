using Molten.Collections;
using Molten.Font;
using Molten.Graphics;
using Molten.Threading;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Reflection;

namespace Molten
{
    /// <summary>Manages the loading, unloading and reusing of content.</summary>
    public class ContentManager : EngineObject
    {
        Dictionary<Type, IContentProcessor> _defaultProcessors;
        Dictionary<Type, IContentProcessor> _customProcessors;
        ConcurrentDictionary<string, ContentHandle> _content;
        ConcurrentDictionary<string, ContentWatcher> _watchers;

        Type[] _defaultServices = new Type[0];
        WorkerGroup _workers;
        Logger _log;
        Engine _engine;
        JsonSerializerSettings _jsonSettings;
        EngineThread _watcherThread;
        bool _hotReloadEnabled;

        private void AddProcessorsFromAssembly(Assembly assembly)
        {
            IEnumerable<Type> types = ReflectionHelper.FindType<IContentProcessor>(assembly);
            foreach (Type t in types)
            {
                IContentProcessor proc = Activator.CreateInstance(t) as IContentProcessor;

                foreach (Type accepted in proc.AcceptedTypes)
                {
                    if (_defaultProcessors.ContainsKey(accepted))
                        continue;

                    _defaultProcessors.Add(accepted, proc);
                }
            }
        }

        /// <summary>Creates a new instance of <see cref="ContentManager"/>.</summary>
        /// <param name="log">A logger to output content information.</param>
        /// <param name="engine">The engine instance to which the content manager will be bound.</param>
        /// <param name="workerThreads">The number of worker threads that will be used to fulfil content requests.</param>
        internal ContentManager(Logger log, Engine engine, int workerThreads, bool hotReloadEnabled)
        {
            string exePath = Assembly.GetEntryAssembly().Location;
            ExecutablePath = new FileInfo(exePath);

            _hotReloadEnabled = true;
            _defaultProcessors = new Dictionary<Type, IContentProcessor>();
            _watchers = new ConcurrentDictionary<string, ContentWatcher>();

            Type t = typeof(IContentProcessor);
            AddProcessorsFromAssembly(t.Assembly);

            _engine = engine;                       

            // Store all the provided custom processors by type.
            _customProcessors = new Dictionary<Type, IContentProcessor>();
            _content = new ConcurrentDictionary<string, ContentHandle>();
            _workers = engine.Threading.CreateWorkerGroup("content workers", workerThreads, paused:true);
            _log = log;

            _jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.None,
                Error = (sender, args) =>
                {
                    log.Error(args.ErrorContext.Error, true);
                    args.ErrorContext.Handled = true;
                },
                Converters = new List<JsonConverter>(),
                CheckAdditionalContent = false,
                Formatting = Formatting.Indented,
            };

            AddCustomJsonConverters(_jsonSettings, engine.Settings.JsonConverters);

            _watcherThread = Engine.Threading.CreateThread("Content Watcher", _hotReloadEnabled, false, ScanContentHandles);
        }

        private void ScanContentHandles(Timing timing)
        {
            foreach (KeyValuePair<string, ContentWatcher> kv in _watchers)
                kv.Value.CheckForChanges();

            Thread.Sleep(Engine.Settings.ContentHotReloadInterval);
        }

        internal ContentWatcher StartWatching(ContentLoadHandle handle)
        {
            string dir = handle.Info.DirectoryName;

            if(!_watchers.TryGetValue(dir, out ContentWatcher watcher))
            {
                watcher = new ContentWatcher(this, new DirectoryInfo(dir));
                _watchers.TryAdd(dir, watcher);
            }

            watcher.Handles.Add(handle);

            return watcher;
        }

        internal bool StopWatching(ContentWatcher watcher, ContentLoadHandle handle)
        {
            string dir = handle.Info.DirectoryName;
            bool removed = false;

            removed = watcher.Handles.Remove(handle);

            // If watcher has no handles to watch, remove the watcher.
            if (watcher.Handles.Count == 0)
            {
                if(_watchers.Remove(dir, out watcher))
                    watcher.Dispose();
            }

            return removed;
        }

        /// <summary>
        /// Adds one or more <see cref="ContentProcessor"/> to the current <see cref="ContentManager"/> instance.
        /// </summary>
        /// <param name="processors"></param>
        public void AddCustomProcessors(params IContentProcessor[] processors)
        {
            if (processors == null || processors.Length == 0)
                return;

            foreach (IContentProcessor p in processors)
            {
                foreach (Type t in p.AcceptedTypes)
                    _customProcessors[t] = p;
            }
        }

        private void AddCustomJsonConverters(JsonSerializerSettings settings, IList<JsonConverter> converters)
        {
            if (converters == null)
                return;

            foreach (JsonConverter jc in converters)
                settings.Converters.Add(jc);
        }

        /// <summary>
        /// Unloads the content represented by the provided <see cref="ContentLoadHandle{T}"/>. If the content has already been unloaded, this method will return false.
        /// </summary>
        /// <typeparam name="T">The type of content to be unloaded.</typeparam>
        /// <param name="handle">The handle of the content to be unloaded.</param>
        /// <returns>True if the content was successfully unloaded, or false if it was already unloaded.</returns>
        public bool Unload(ContentLoadHandle handle)
        {
            if(handle.Status != ContentHandleStatus.Unloaded)
            {
                handle.Status = ContentHandleStatus.Unloaded;

                if (_content.Remove(handle.RelativePath, out ContentHandle cHandle))
                {
                    if (handle.Asset is IDisposable disposable)
                        disposable.Dispose();

                    return true;
                }
            }

            return false;
        }

        public ContentLoadBatch GetLoadBatch()
        {
            return new ContentLoadBatch(this);
        }

        /// <summary>
        /// A helper method for loading a <see cref="TextFont"/> instance the provided path or system font name.
        /// </summary>
        /// <param name="path">The font file path, or system font name. e.g. "Arial" or "Times New Roman"</param>
        /// <param name="completionCallback"></param>
        /// <param name="parameters"></param>
        /// <param name="canHotReload"></param>
        /// <param name="dispatch"></param>
        /// <returns></returns>
        public ContentLoadHandle LoadFont(string path, ContentLoadCallbackHandler<TextFont> completionCallback = null, TextFontParameters parameters = null, bool canHotReload = true, bool dispatch = true)
        {
            if (!File.Exists(path))
            {
                FileInfo fInfo = new FileInfo(path);
                string sysFontName = fInfo.Name;

                if (!string.IsNullOrEmpty(fInfo.Extension))
                    sysFontName = sysFontName.Replace(fInfo.Extension, "");

                path = FontFile.GetSystemFontPath(sysFontName);
            }

            return Load(path, completionCallback, parameters, canHotReload, dispatch);
        }

        public ContentLoadHandle Load<T>(string path, ContentLoadCallbackHandler<T> completionCallback = null, ContentParameters parameters = null, bool canHotReload = true, bool dispatch = true)
        {
            Type contentType = typeof(T);
            return Load(contentType, path, 
                (asset, isReload) => completionCallback?.Invoke((T)asset, isReload), 
                parameters, canHotReload, dispatch);
        }

        public ContentLoadHandle Load(Type contentType, string path, ContentLoadCallbackHandler<object> completionCallback = null, ContentParameters parameters = null, bool canHotReload = true, bool dispatch = true)
        {
            IContentProcessor proc = GetProcessor(path, contentType);
            parameters = ValidateParameters(path, proc, parameters);

            if (proc == null)
            {
                _log.Error($"[CONTENT] {path}: Unable to load unsupported content of type '{contentType.Name}'");
                return null;
            }

            if (!_content.TryGetValue(path, out ContentHandle handle))
            {
                handle = new ContentLoadHandle(this, path, parameters.PartCount, contentType, proc, parameters, completionCallback, canHotReload);
                _content.TryAdd(path, handle);

                if (dispatch)
                    handle.Dispatch();

            }

            return handle as ContentLoadHandle;
        }

        public ContentLoadJsonHandle Deserialize<T>(string path, ContentLoadCallbackHandler<T> completionCallback = null, JsonSerializerSettings settings = null, bool canHotReload = true, bool dispatch = true)
        {
            Type contentType = typeof(T);
            return Deserialize(contentType, path,
                (asset, isReload) => completionCallback?.Invoke((T)asset, isReload), 
                settings, canHotReload, dispatch);
        }

        public ContentLoadJsonHandle Deserialize(Type contentType, string path, ContentLoadCallbackHandler<object> completionCallback = null, JsonSerializerSettings settings = null, bool canHotReload = true, bool dispatch = true)
        {
            if (!_content.TryGetValue(path, out ContentHandle handle))
            {
                settings = settings ?? _jsonSettings;
                handle = new ContentLoadJsonHandle(this, path, contentType, completionCallback, settings, canHotReload);
                _content.TryAdd(path, handle);

                if (dispatch)
                    handle.Dispatch();
            }

            return handle as ContentLoadJsonHandle;
        }

        public ContentSaveHandle SaveToFile(string path, object asset, Action<FileInfo> completionCallback = null, ContentParameters parameters = null, bool dispatch = true)
        {
            Type contentType = asset.GetType();
            IContentProcessor proc = GetProcessor(path, contentType);

            if (proc == null)
            {
                _log.Error($"[CONTENT] {path}: Unable to load unsupported content of type '{contentType.Name}'");
                return null;
            }

            ContentSaveHandle handle = new ContentSaveHandle(this, path, asset, proc, parameters, completionCallback);

            if (dispatch)
                handle.Dispatch();

            return handle;
        }

        public ContentSaveJsonHandle SerializeToFile(string path, object asset, Action<FileInfo> completionCallback = null, JsonSerializerSettings settings = null, bool dispatch = true)
        {
            settings = settings ?? _jsonSettings;
            ContentSaveJsonHandle handle = new ContentSaveJsonHandle(this, path, asset, settings, completionCallback);
            
            if(dispatch)
                handle.Dispatch();

            return handle;
        }

        private ContentParameters ValidateParameters(string filePath, IContentProcessor processor, ContentParameters parameters)
        {
            if (processor == null)
                return null;

            if (parameters != null)
            {
                Type pType = parameters.GetType();

                if (!processor.ParameterType.IsAssignableFrom(pType))
                    Log.Warning($"[Content] {filePath}: Invalid parameter type provided. Expected '{processor.ParameterType.Name}' but received '{pType.Name}'. Using defaults instead.");
                else
                    return parameters;
            }

            return Activator.CreateInstance(processor.ParameterType) as ContentParameters;
        }

        internal IContentProcessor GetProcessor(string path, Type type)
        {
            IContentProcessor proc = null;
            if (_customProcessors.TryGetValue(type, out proc) || 
                _defaultProcessors.TryGetValue(type, out proc))
            {
                bool hasAllServices = true;

                Type[] requiredServices = proc.RequiredServices ?? _defaultServices;

                // Found matching content processor, but check if all required services are present.
                foreach (Type serviceType in requiredServices)
                {
                    if (!_engine.IsServiceAvailable(serviceType))
                    {
                        hasAllServices = false;
                        Log.Error($"[CONTENT] {path}: missing required service '{serviceType.Name}' for viable content processor '{proc.GetType().Name}'");
                        break;
                    }
                }

                if (hasAllServices)
                    return proc;
                else
                    proc = null;
            }

            if (type.BaseType != null)
            {
                if (type.BaseType != typeof(EngineObject))
                {
                    proc = GetProcessor(path, type.BaseType);
                    return proc;
                }
                else
                {
                    return null;
                }
            }

            IEnumerable<Type> baseInterfaces = type.GetInterfaces();
            foreach (Type iType in baseInterfaces)
            {
                proc = GetProcessor(path, iType);
                if (proc != null)
                    return proc;
            }
 
            return proc;
        }
        protected override void OnDispose()
        {
            _workers.Dispose();

            foreach (ContentWatcher watcher in _watchers.Values)
                watcher.Dispose();

            _watchers.Clear();
            _watcherThread.Dispose();
        }

        /// <summary>
        /// Gets the bound <see cref="Logger"/>.
        /// </summary>
        internal Logger Log => _log;

        /// <summary>
        /// Gets the bound <see cref="Engine"/> instance.
        /// </summary>
        internal Engine Engine => _engine;

        /// <summary>
        /// Gets the <see cref="WorkerGroup"/> used by the current <see cref="ContentManager"/> for loading and processing content.
        /// </summary>
        internal WorkerGroup Workers => _workers;

        /// <summary>
        /// Gets file information for the current executable.
        /// </summary>
        internal FileInfo ExecutablePath { get; }

        /// <summary>
        /// Gets or sets whether hot-reloading of content is allowed with the current <see cref="ContentManager"/>.
        /// </summary>
        public bool HotReloadEnabled
        {
            get => _hotReloadEnabled;
            set
            {
                if(_hotReloadEnabled != value)
                {
                    _hotReloadEnabled = value;
                    if (_hotReloadEnabled)
                        _watcherThread.Start();
                    else
                        _watcherThread.Pause();
                }
            }
        }
    }
}
