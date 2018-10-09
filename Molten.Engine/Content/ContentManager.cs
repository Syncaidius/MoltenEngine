using Molten.Collections;
using Molten.Content;
using Molten.Threading;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Molten
{
    /// <summary>Manages the loading, unloading and reusing of content.</summary>
    public class ContentManager : EngineObject
    {
        static Dictionary<Type, ContentProcessor> _defaultProcessors;
        static ObjectPool<ContentRequest> _requestPool;
        internal static ObjectPool<ContentContext> ContextPool;

        Dictionary<Type, ContentProcessor> _customProcessors;
        ThreadedDictionary<string, ContentFile> _content;
        ThreadedDictionary<string, ContentDirectory> _directories;

        WorkerGroup _workers;
        Logger _log;
        Engine _engine;
        JsonSerializerSettings _jsonSettings;

        static ContentManager()
        {
            _defaultProcessors = new Dictionary<Type, ContentProcessor>();
            _requestPool = new ObjectPool<ContentRequest>(() => new ContentRequest());
            ContextPool = new ObjectPool<ContentContext>(() => new ContentContext());

            Type t = typeof(ContentProcessor);
            AddProcessorsFromAssembly(t.Assembly);
        }

        internal static void AddProcessorsFromAssembly(Assembly assembly)
        {
            IEnumerable<Type> types = ReflectionHelper.FindType<ContentProcessor>(assembly);
            foreach (Type t in types)
            {
                ContentProcessor proc = Activator.CreateInstance(t) as ContentProcessor;
                foreach (Type accepted in proc.AcceptedTypes)
                {
                    if (_defaultProcessors.ContainsKey(accepted))
                        continue;
                    else
                        _defaultProcessors.Add(accepted, proc);
                }
            }
        }

        /// <summary>Creates a new instance of <see cref="ContentManager"/>.</summary>
        /// <param name="customProcessors">A list of custom processors to add to the current <see cref="ContentManager"/>.</param>
        /// <param name="log">A logger to output content information.</param>
        /// <param name="engine">The engine instance to which the content manager will be bound.</param>
        /// <param name="workerThreads">The number of worker threads that will be used to fulfil content requests.</param>
        /// <param name="customJsonConverters"></param>
        internal ContentManager(Logger log, Engine engine, IList<ContentProcessor> customProcessors = null, int workerThreads = 1)
        {
            _engine = engine;

            // Store all the provided custom processors by type.
            _customProcessors = new Dictionary<Type, ContentProcessor>();
            _content = new ThreadedDictionary<string, ContentFile>();
            _directories = new ThreadedDictionary<string, ContentDirectory>();
            _workers = engine.Threading.SpawnWorkerGroup("content workers", workerThreads);
            _log = log;
            _jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.None,
                Error = (sender, args) =>
                {
                    log.WriteError(args.ErrorContext.Error, true);
                    args.ErrorContext.Handled = true;
                },
                Converters = new List<JsonConverter>(),
                CheckAdditionalContent = false,
                Formatting = Formatting.Indented,
            };

            AddCustomJsonConverters(_jsonSettings, engine.Settings.JsonConverters);

            AddCustomProcessors(engine.Settings.CustomContentProcessors);
            AddCustomProcessors(customProcessors);
        }

        private void AddCustomProcessors(IList<ContentProcessor> processors)
        {
            if (processors == null)
                return;

            foreach (ContentProcessor p in processors)
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
        public ContentRequest BeginRequest(string rootDirectory)
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
            if(request.RequestElements.Count == 0)
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
                    _log.WriteWarning($"[CONTENT] [RELOAD] Unable to reload {file.Path}. Not loaded via content manager.");
                    return;
                }

                foreach (Type t in types)
                {
                    IList<object> existingObjects = file.GetObjects(t);
                    context.Input.Add(t, new List<object>(existingObjects));
                    _log.WriteLine($"[CONTENT] [RELOAD] {file.Path}");
                    DoRead(null, context, file.OriginalProcessor);
                }
            }
            else if (file.OriginalRequestType == ContentRequestType.Deserialize)
            {
                foreach(Type t in types)
                {
                    IList<object> existingObjects = file.GetObjects(t);
                    context.Input.Add(t, new List<object>(existingObjects));
                    _log.WriteLine($"[CONTENT] [RELOAD] {file.Path}");
                    DoDeserialize(null, context);
                }
            }
        }

        private ContentFile GetContentFile(ContentContext context, ContentRequest request = null, ContentProcessor processor = null)
        {
            ContentFile file;
            string fnLower = context.Filename.ToLower();

            if (!_content.TryGetValue(fnLower, out file))
            {
                file = new ContentFile()
                {
                    OriginalProcessor = processor,
                    OriginalContentType = context.ContentType,
                    File = context.File,
                    OriginalRequestType = context.RequestType,
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

        internal ContentProcessor GetProcessor(Type type)
        {
            ContentProcessor proc = null;
            if (_customProcessors.TryGetValue(type, out proc) || _defaultProcessors.TryGetValue(type, out proc))
                return proc;
            else
            {
                if (type.BaseType != null)
                {
                    proc = GetProcessor(type.BaseType);
                    return proc;
                }

                IEnumerable<Type> baseInterfaces = type.GetInterfaces();
                foreach (Type iType in baseInterfaces)
                {
                    proc = GetProcessor(iType);
                    if (proc != null)
                        return proc;
                }
            }

            return proc;
        }

        internal void ProcessRequest(ContentRequest request)
        {
            foreach (ContentContext context in request.RequestElements)
            {
                ContentProcessor proc = null;

                // First check if the content already exists
                string fnLower = context.Filename.ToLower();
                if (_content.TryGetValue(fnLower, out ContentFile file))
                {
                    object existing = file.GetObject(_engine, context.ContentType, context.Metadata);
                    if (existing != null)
                    {
                        request.RetrievedContent[fnLower] = file;
                        continue;
                    }
                }

                if (context.RequestType != ContentRequestType.Deserialize && context.RequestType != ContentRequestType.Serialize)
                {
                    proc = proc ?? GetProcessor(context.ContentType);
                    if (proc == null)
                    {
                        _log.WriteError($"[CONTENT] {context.File}: Unable to load unsupported content of type '{context.ContentType.Name}'");
                        continue;
                    }
                }

                context.Engine = _engine;
                context.Log = _log;

                try
                {
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
                catch(Exception ex)
                {
                    _log.WriteError($"An error occurred while processing content {context.Filename}");
                    _log.WriteError(ex, true);
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
                    _log.WriteLine($"[CONTENT] [DESERIALIZE] '{result.GetType().Name}' from {context.Filename}");
                }
                catch (Exception ex)
                {
                    _log.WriteError($"[CONTENT] [DESERIALIZE] { ex.Message}", context.Filename);
                    _log.WriteError(ex, true);
                }
            }
        }

        private void DoRead(ContentRequest request, ContentContext context, ContentProcessor proc)
        {
            proc.OnRead(context);

            if (context.Output.Count > 0)
            {
                _log.WriteLine($"[CONTENT] [READ] {context.Filename}:");

                foreach (Type t in context.Output.Keys)
                {
                    ContentFile file = GetContentFile(context, request, proc);
                    List<object> result = context.Output[t];
                    file.AddObjects(t, result);
                    _log.WriteLine($"[CONTENT] [READ]    {result.Count}x {t.FullName}");
                }
            }
        }

        private void DoWrite(ContentContext context, ContentProcessor proc)
        {
             proc.OnWrite(context);
            _log.WriteLine($"[CONTENT] [WRITE] {context.Filename}");
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

                _log.WriteLine($"[CONTENT] [SERIALIZE] {context.Filename}");
            }
            catch (Exception ex)
            {
                _log.WriteError($"[CONTENT] [SERIALIZE] { ex.Message}", context.Filename);
                _log.WriteError(ex, true);
            }
        }

        protected override void OnDispose()
        {
            _workers.Dispose();

            ICollection<ContentFile> files = _content.Values;
            foreach (ContentFile file in files)
                file.Dispose();

            base.OnDispose();
        }

        internal Logger Log => _log;

        internal Engine Engine => _engine;
    }
}
