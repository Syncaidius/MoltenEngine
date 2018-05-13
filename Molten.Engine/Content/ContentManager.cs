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
        internal static char[] REQUEST_SPLITTER = new char[] { ';' };
        internal static char[] METADATA_ASSIGNMENT = new char[] { '=' };

        static Dictionary<Type, ContentProcessor> _defaultProcessors;
        static ObjectPool<ContentRequest> _requestPool;
        internal static ObjectPool<ContentContext> ContextPool;

        Dictionary<Type, ContentProcessor> _customProcessors;
        Dictionary<string, ContentFile> _content;
        Dictionary<string, ContentDirectory> _directories;

        int _lockerVal;
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
        /// <param name="rootDirectory">The root directory of the new <see cref="ContentManager"/> instance.</param>
        /// <param name="customProcessors">A list of custom processors to override the default ones with. Default value is null.</param>
        /// <param name="engine"></param>
        /// <param name="workerThreads">The number of worker threads that will be used to fulfil content requests.</param>
        public ContentManager(Logger log, Engine engine, List<ContentProcessor> customProcessors = null, int workerThreads = 1, JsonSerializerSettings jsonSettings = null)
        {
            _engine = engine;

            // Store all the provided custom processors by type.
            _customProcessors = new Dictionary<Type, ContentProcessor>();
            _content = new Dictionary<string, ContentFile>();
            _directories = new Dictionary<string, ContentDirectory>();
            _workers = engine.Threading.SpawnWorkerGroup("content workers", workerThreads);
            _log = log;
            _jsonSettings = jsonSettings ?? JsonHelper.GetDefaultSettings(_log);

            if(customProcessors != null)
            {
                foreach(ContentProcessor p in customProcessors)
                {
                    foreach(Type t in p.AcceptedTypes)
                    {
                        if (_customProcessors.ContainsKey(t))
                            continue;
                        else
                            _customProcessors.Add(t, p);
                    }
                }
            }
        }

        /// <summary>
        /// Spawns a new content request and returns it for the provided directory.
        /// </summary>
        /// <param name="rootDirectory">The root directory of all operations added to the request.</param>
        /// <returns></returns>
        public ContentRequest StartRequest(string rootDirectory)
        {
            ContentRequest request = _requestPool.GetInstance();
            request.RootDirectory = Path.GetFullPath(rootDirectory.StartsWith("/") ? rootDirectory.Substring(1, rootDirectory.Length - 1) : rootDirectory);
            request.Manager = this;
            return request;
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
            if (_content.TryGetValue(e.FullPath.ToLower(), out ContentFile file))
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

        }

        private ContentSegment GetSegment(ContentContext context, ContentRequest request = null)
        {
            ContentFile file;
            string fnLower = context.Filename.ToLower();

            if (!_content.TryGetValue(fnLower, out file))
            {
                file = new ContentFile(context.Filename, context.RequestType);

                ContentDirectory directory;
                string strDirectory = context.File.Directory.ToString();
                if (!_directories.TryGetValue(strDirectory, out directory))
                {
                    directory = new ContentDirectory(strDirectory);
                    directory.OnChanged += Watcher_Changed;
                    _directories.Add(strDirectory, directory);
                }

                _content.Add(context.Filename, file);
                directory.AddFile(file);
            }

            if (request != null)
                request.RetrievedContent[fnLower] = file;

            ContentSegment segment;
            if(!file.Segments.TryGetValue(context.ContentType, out segment))
            {
                segment = new ContentSegment(context.ContentType, context.Filename);              
                file.Segments.Add(context.ContentType, segment);
            }

            return segment;
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
            ContentProcessor proc = null;
            foreach (ContentContext context in request.RequestElements)
            {
                if (request.State == ContentRequestState.Cancelled)
                    return;

                proc = GetProcessor(context.ContentType);
                if (proc == null)
                {
                    _log.WriteError($"[CONTENT] {context.File}: Unable to load unsupported content of type '{context.ContentType.Name}'");
                    continue;
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

                        case ContentRequestType.Write:
                            using (context.Stream = new FileStream(context.Filename, FileMode.Create, FileAccess.Write))
                            {
                                proc.OnWrite(context);
                            }
                            break;

                        case ContentRequestType.Delete:
                            context.File.Delete();
                            break;
                    }
                }
                catch(Exception ex)
                {
                    _log.WriteError(ex, true);
                }

                ContextPool.Recycle(context);
            }

            request.Complete();
            _requestPool.Recycle(request);
        }

        private void DoDeserialize(ContentRequest request, ContentContext context)
        {
            using (FileStream stream = new FileStream(context.Filename, FileMode.Open, FileAccess.Read))
            {
                string json;
                using (StreamReader reader = new StreamReader(stream))
                    json = reader.ReadToEnd();

                try
                {
                    object result = JsonConvert.DeserializeObject(json, context.ContentType, _jsonSettings);
                    ContentSegment segment = GetSegment(context, request);
                    segment.Objects.Add(result);
                    _log.WriteLine($"[CONTENT] [JSON] Loaded '{result.GetType().Name}' from '{context.Filename}'");
                }
                catch (Exception ex)
                {
                    _log.WriteError($"[CONTENT] [JSON] { ex.Message}", context.Filename);
                }
            }
        }

        private void DoRead(ContentRequest request, ContentContext context, ContentProcessor proc)
        {
            using (context.Stream = new FileStream(context.Filename, FileMode.Open, FileAccess.Read))
                proc.OnRead(context);

            if(context.Output.Count > 0)
            {
                _log.WriteLine($"[CONTENT] Loaded from '{context.Filename}':");

                ThreadingHelper.InterlockSpinWait(ref _lockerVal, () =>
                {
                    foreach (Type t in context.Output.Keys)
                    {
                        ContentSegment segment = GetSegment(context, request);
                        List<object> result = context.Output[t];
                        segment.Objects.AddRange(result);
                        for (int i = 0; i < result.Count; i++)
                            _log.WriteLine($"[CONTENT]    {result[i].GetType().Name} - {result[i].ToString()}");
                    }
                });
            }
        }

        protected override void OnDispose()
        {
            _workers.Dispose();
            base.OnDispose();
        }

        internal Logger Log => _log;

        internal Engine Engine => _engine;
    }
}
