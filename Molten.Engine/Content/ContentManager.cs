using Molten.Collections;
using Molten.Threading;
using Newtonsoft.Json;
using System.Reflection;

namespace Molten
{
    /// <summary>Manages the loading, unloading and reusing of content.</summary>
    public class ContentManager : EngineObject
    {
        Dictionary<Type, IContentProcessor> _defaultProcessors;
        ObjectPool<ContentRequest> _requestPool;
        internal ObjectPool<ContentContext> ContextPool;
        Dictionary<Type, IContentProcessor> _customProcessors;
        ThreadedDictionary<string, ContentFile> _content;
        ThreadedDictionary<string, ContentDirectory> _directories;

        Type[] _defaultServices = new Type[0];
        WorkerGroup _workers;
        Logger _log;
        Engine _engine;
        JsonSerializerSettings _jsonSettings;

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
        internal ContentManager(Logger log, Engine engine, int workerThreads = 1)
        {
            _defaultProcessors = new Dictionary<Type, IContentProcessor>();

            Type t = typeof(IContentProcessor);
            AddProcessorsFromAssembly(t.Assembly);

            _engine = engine;            
            _requestPool = new ObjectPool<ContentRequest>(() => new ContentRequest());
            ContextPool = new ObjectPool<ContentContext>(() => new ContentContext());            

            // Store all the provided custom processors by type.
            _customProcessors = new Dictionary<Type, IContentProcessor>();
            _content = new ThreadedDictionary<string, ContentFile>();
            _directories = new ThreadedDictionary<string, ContentDirectory>();
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
        /// Spawns a new content request and returns it for the provided directory.
        /// </summary>
        /// <param name="rootDirectory">The root directory of all operations added to the request.</param>
        /// <returns></returns>
        public ContentRequest BeginRequest(string rootDirectory = null)
        {
            return BeginRequest(rootDirectory, null);
        }

        /// <summary>
        /// Spawns a new content request and returns it for the provided directory.
        /// </summary>
        /// <param name="rootDirectory">The root directory of all operations added to the request.</param>
        /// <param name="customJsonSettings">Custom Json settings to be applied to any serialization or deserialization operation performed by the <see cref="ContentRequest"/>.</param>
        /// <returns></returns>
        public ContentRequest BeginRequest(string rootDirectory, JsonSerializerSettings customJsonSettings)
        {
            ContentRequest request = _requestPool.GetInstance();

            if (customJsonSettings != null)
            {
                request.JsonSettings = customJsonSettings.Clone();
                AddCustomJsonConverters(request.JsonSettings, _jsonSettings.Converters);
            }
            else
            {
                request.JsonSettings = _jsonSettings;
            }

            if (string.IsNullOrEmpty(rootDirectory))
                rootDirectory = Engine.Settings.DefaultAssetPath;

            rootDirectory = rootDirectory.StartsWith("/") ? rootDirectory.Substring(1, rootDirectory.Length - 1) : rootDirectory;
            request.RootDirectory = Path.GetFullPath(rootDirectory);
            request.Manager = this;
            return request;
        }

        internal void CommitImmediate(ContentRequest request)
        {
            if (request.RequestElements.Count == 0)
            {
                request.Complete();
                _requestPool.Recycle(request);
                return;
            }

            ProcessRequest(request);
        }

        internal void Commit(ContentRequest request)
        {
            if (request.RequestElements.Count == 0)
            {
                request.Complete();
                _requestPool.Recycle(request);
                return;
            }

            ContentWorkerTask task = ContentWorkerTask.Get();
            task.Request = request;
            _workers.QueueTask(task);
        }

        private void Watcher_Changed(ContentDirectory dir, FileSystemEventArgs e)
        {
            string pathLower = e.FullPath.ToLower();
            if (_content.TryGetValue(pathLower, out ContentFile file))
            {
                ContentReloadTask task = ContentReloadTask.Get();
                task.File = file;
                task.Manager = this;
                _workers.QueueTask(task);
            }
        }

        internal void ReloadFile(ContentFile file)
        {
            ContentContext context = ContextPool.GetInstance();
            context.File = file.File;
            context.ContentType = file.OriginalContentType;

            // Create a local copy of the keys to avoid threading issues
            Type[] types = file.GetTypeArray();

            // Add as input objects, all that were loaded from the original version of the file.
            // It is up to the content processor to update existing object instances and output them.
            if (file.OriginalRequestType == ContentRequestType.Read)
            {
                if (file.OriginalProcessor == null)
                {
                    _log.Warning($"[CONTENT] [RELOAD] Unable to reload {file.Path}. Not loaded via content manager.");
                    return;
                }

                foreach (Type t in types)
                {
                    IList<object> existingObjects = file.GetObjects(t);
                    context.Input.Add(t, new List<object>(existingObjects));
                    _log.Log($"[CONTENT] [RELOAD] {file.Path}");
                    DoRead(null, context, file.OriginalProcessor);
                }
            }
            else if (file.OriginalRequestType == ContentRequestType.Deserialize)
            {
                foreach (Type t in types)
                {
                    IList<object> existingObjects = file.GetObjects(t);
                    context.Input.Add(t, new List<object>(existingObjects));
                    _log.Log($"[CONTENT] [RELOAD] {file.Path}");
                    DoDeserialize(null, context);
                }
            }
        }

        private ContentFile GetContentFile(ContentContext context, ContentRequest request = null, IContentProcessor processor = null)
        {
            string fnLower = context.Filename.ToLower();

            if (!_content.TryGetValue(fnLower, out ContentFile file))
            {
                file = new ContentFile()
                {
                    OriginalProcessor = processor,
                    OriginalContentType = context.ContentType,
                    File = context.File,
                    OriginalRequestType = context.RequestType,
                    Parameters = context.Parameters,
                };

                ContentDirectory directory;
                string strDirectory = context.File.Directory.ToString();
                if (!_directories.TryGetValue(strDirectory, out directory))
                {
                    directory = new ContentDirectory(strDirectory);
                    directory.OnChanged += Watcher_Changed;
                    _directories.Add(strDirectory, directory);
                }

                _content.Add(fnLower, file);
                directory.AddFile(file);
            }

            if (request != null)
                request.RetrievedContent[fnLower] = file;

            return file;
        }

        internal IContentProcessor GetProcessor(ContentContext context, Type type)
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
                        Log.Error($"[CONTENT] {context.File}: missing required service '{serviceType.Name}' for viable content processor '{proc.GetType().Name}'");
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
                    proc = GetProcessor(context, type.BaseType);
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
                proc = GetProcessor(context, iType);
                if (proc != null)
                    return proc;
            }
 
            return proc;
        }

        internal void ProcessRequest(ContentRequest request)
        {
            foreach (ContentContext context in request.RequestElements)
            {
                context.Engine = _engine;
                context.Log = _log;

                IContentProcessor proc = null;

                // First check if the content already exists
                string fnLower = context.Filename.ToLower();
                if (_content.TryGetValue(fnLower, out ContentFile file))
                {
                    ValidateParameters(context, file.OriginalProcessor);

                    object existing = file.GetObject(_engine, context.ContentType, context.Parameters);
                    if (existing != null)
                    {
                        request.RetrievedContent[fnLower] = file;
                        continue;
                    }
                }

                if (context.RequestType != ContentRequestType.Deserialize && 
                    context.RequestType != ContentRequestType.Serialize)
                {
                    proc = proc ?? GetProcessor(context, context.ContentType);
                    if (proc == null)
                    {
                        _log.Error($"[CONTENT] {context.File}: Unable to load unsupported content of type '{context.ContentType.Name}'");
                        continue;
                    }
                }

                try
                {
                    ValidateParameters(context, proc);

                    switch (context.RequestType)
                    {
                        case ContentRequestType.Read:
                            DoRead(request, context, proc);
                            break;

                        case ContentRequestType.Deserialize:
                            DoDeserialize(request, context);
                            break;

                        case ContentRequestType.Serialize:
                            DoSerialize(request, context);
                            break;

                        case ContentRequestType.Write:
                            DoWrite(context, proc);
                            break;

                        case ContentRequestType.Delete:
                            context.File.Delete();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"An error occurred while processing content {context.Filename}");
                    _log.Error(ex, true);
                }

                ContextPool.Recycle(context);
            }

            request.Complete();
            _requestPool.Recycle(request);
        }

        private void DoDeserialize(ContentRequest request, ContentContext context)
        {
            using (Stream stream = new FileStream(context.Filename, FileMode.Open, FileAccess.Read))
            {
                string json;
                using (StreamReader reader = new StreamReader(stream))
                    json = reader.ReadToEnd();

                try
                {
                    object result = JsonConvert.DeserializeObject(json, context.ContentType, request.JsonSettings);
                    ContentFile file = GetContentFile(context, request);
                    file.AddObject(context.ContentType, result);
                    _log.Log($"[CONTENT] [DESERIALIZE] '{result.GetType().Name}' from {context.Filename}");
                }
                catch (Exception ex)
                {
                    _log.Error($"[CONTENT] [DESERIALIZE] { ex.Message}", context.Filename);
                    _log.Error(ex, true);
                }
            }
        }

        private void DoRead(ContentRequest request, ContentContext context, IContentProcessor proc)
        {
            proc.Read(context);

            if (context.Output.Count > 0)
            {
                _log.Log($"[CONTENT] [READ] {context.Filename}:");

                foreach (Type t in context.Output.Keys)
                {
                    ContentFile file = GetContentFile(context, request, proc);
                    List<object> result = context.Output[t];
                    file.AddObjects(t, result);
                    _log.Log($"[CONTENT] [READ]    {result.Count}x {t.FullName}");
                }
            }
        }

        private void DoWrite(ContentContext context, IContentProcessor proc)
        {
            proc.Write(context);
            _log.Log($"[CONTENT] [WRITE] {context.Filename}");
        }

        private void DoSerialize(ContentRequest request, ContentContext context)
        {
            try
            {
                string json = JsonConvert.SerializeObject(context.Input[context.ContentType][0], request.JsonSettings);

                if (!context.File.Directory.Exists)
                    context.File.Directory.Create();

                using (Stream stream = new FileStream(context.Filename, FileMode.Create, FileAccess.Write))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(json);
                    }
                }

                _log.Log($"[CONTENT] [SERIALIZE] {context.Filename}");
            }
            catch (Exception ex)
            {
                _log.Error($"[CONTENT] [SERIALIZE] { ex.Message}", context.Filename);
                _log.Error(ex, true);
            }
        }

        private void ValidateParameters(ContentContext context, IContentProcessor processor)
        {
            Type pExpectedType = processor.GetParameterType();

            if (context.Parameters != null)
            {
                Type pType = context.Parameters.GetType();

                if (!pExpectedType.IsAssignableFrom(pType))
                    _log.Warning($"[CONTENT] {context.File}: Invalid parameter type provided. Expected '{pExpectedType.Name}' but received '{pType.Name}'. Using defaults instead.");
                else
                    return;
            }

            context.Parameters = Activator.CreateInstance(pExpectedType) as IContentParameters;
        }

        protected override void OnDispose()
        {
            _workers.Dispose();

            ICollection<ContentFile> files = _content.Values;
            foreach (ContentFile file in files)
                file.Dispose();
        }

        /// <summary>
        /// Gets the bound <see cref="Logger"/>.
        /// </summary>
        internal Logger Log => _log;

        /// <summary>
        /// Gets the bound <see cref="Engine"/> instance.
        /// </summary>
        internal Engine Engine => _engine;

        internal WorkerGroup Workers => _workers;
    }
}
