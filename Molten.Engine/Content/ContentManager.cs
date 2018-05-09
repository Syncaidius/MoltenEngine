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
using System.Threading.Tasks;

namespace Molten
{
    /// <summary>Manages the loading, unloading and reusing of content.</summary>
    public class ContentManager : EngineObject
    {
        internal static char[] REQUEST_SPLITTER = new char[] { ';' };
        internal static char[] METADATA_ASSIGNMENT = new char[] { '=' };

        static Dictionary<Type, ContentProcessor> _defaultProcessors;
        internal static ObjectPool<ContentRequestElement> ElementPool;
        static ObjectPool<ContentRequest> _requestPool;

        Dictionary<Type, ContentProcessor> _customProcessors;
        Dictionary<Type, ContentTypeGroup> _content;
        int _lockerVal;
        WorkerGroup _workers;
        Logger _log;
        string _rootDirectory;
        Engine _engine;
        JsonSerializerSettings _jsonSettings;

        static ContentManager()
        {
            _defaultProcessors = new Dictionary<Type, ContentProcessor>();
            _requestPool = new ObjectPool<ContentRequest>(() => new ContentRequest());
            ElementPool = new ObjectPool<ContentRequestElement>(() => new ContentRequestElement());
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
        public ContentManager(Logger log, Engine engine, string rootDirectory, List<ContentProcessor> customProcessors = null, int workerThreads = 1, JsonSerializerSettings jsonSettings = null)
        {
            _engine = engine;
            _rootDirectory = rootDirectory.StartsWith("/") ? rootDirectory.Substring(1, rootDirectory.Length - 1) : rootDirectory;

            // Store all the provided custom processors by type.
            _customProcessors = new Dictionary<Type, ContentProcessor>();
            _content = new Dictionary<Type, ContentTypeGroup>();
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

        public ContentRequest StartRequest()
        {
            ContentRequest request = _requestPool.GetInstance();
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

        public T Get<T>(string requestString)
        {
            Dictionary<string, string> meta = new Dictionary<string, string>();
            string path = ParseRequestString(requestString, meta);
            path = path.ToLower();

            Type t = typeof(T);
            ContentTypeGroup group;

            if (!_content.TryGetValue(t, out group))
                return default(T);

            ContentSegment contentFile;
            if (!group.Files.TryGetValue(path, out contentFile))
                return default(T);

            // Since the content exists, we know there's either a custom or default processor for it.
            ContentProcessor proc = GetProcessor(t);

            if (proc != null)
            {
                T obj = (T)proc.OnGet(_engine, t, meta, contentFile.Objects);
                return obj;
            }
            else
            {
                _log.WriteError($"[CONTENT] [GET] unable to fulfil content request from {path}: No content processor available for type {t} or any of its derivatives.");
                return default(T);
            }
        }

        internal string ParseRequestString(string requestString, Dictionary<string, string> metadataOut)
        {
            string[] parts = requestString.Split(ContentManager.REQUEST_SPLITTER, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return requestString;

            string path = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string[] metaParts = parts[i].Split(ContentManager.METADATA_ASSIGNMENT, StringSplitOptions.RemoveEmptyEntries);
                if (metaParts.Length != 2)
                {
                    _log.WriteError($"Invalid metadata segment in content request: {parts[i]}");
                    continue;
                }
                metadataOut.Add(metaParts[0], metaParts[1]);
            }

            return path;
        }

        private ContentSegment GetContentFile(string fn, Type type)
        {
            fn = fn.ToLower();
            ContentTypeGroup group;

            if(!_content.TryGetValue(type, out group))
            {
                group = new ContentTypeGroup(type);
                _content.Add(type, group);
            }

            ContentSegment contentFile;
            if(!group.Files.TryGetValue(fn, out contentFile))
            {
                contentFile = new ContentSegment(fn);
                group.Files.Add(fn, contentFile);
            }

            return contentFile;
        }

        private ContentProcessor GetProcessor(Type type)
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
            foreach (ContentRequestElement e in request.RequestElements)
            {
                if (request.State == ContentRequestState.Cancelled)
                    return;

                proc = GetProcessor(e.ContentType);
                if (proc == null)
                {
                    _log.WriteError($"[CONTENT] {e.Info}: Unable to load unsupported content of type '{e.ContentType.Name}'");
                    continue;
                }

                string filePath = e.Info.ToString();

                try
                {
                    switch (e.Type)
                    {
                        case ContentRequestType.Read:
                            DoRead(e, proc);
                            break;

                        case ContentRequestType.Deserialize:
                            DoDeserialize(e);
                            break;

                        case ContentRequestType.Write:
                            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                                proc.OnWrite(_engine, _log, e.ContentType, stream, e.Info);
                            break;

                        case ContentRequestType.Delete:
                            File.Delete(filePath);
                            break;
                    }
                }
                catch(Exception ex)
                {
                    _log.WriteError(ex, true);
                }
            }

            request.Complete();
            _requestPool.Recycle(request);
        }

        private void DoDeserialize(ContentRequestElement e)
        {
            string path = e.Info.ToString();
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                string json;
                using (StreamReader reader = new StreamReader(stream))
                    json = reader.ReadToEnd();

                try
                {
                    object result = JsonConvert.DeserializeObject(json, e.ContentType, _jsonSettings);
                    ContentSegment group = GetContentFile(e.FilePathString, e.ContentType);
                    group.Objects.Add(result);
                    _log.WriteLine($"[CONTENT] [JSON] Loaded '{result.GetType().Name}' from '{path}'");
                }
                catch (Exception ex)
                {
                    _log.WriteError($"[CONTENT] [JSON] { ex.Message}", path);
                }
            }
        }

        private void DoRead(ContentRequestElement e, ContentProcessor proc)
        {
            string path = e.Info.ToString();
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                proc.OnRead(_engine, _log, e.ContentType, stream, e.Metadata, e.Info, e.Result);

            if(e.Result.Objects.Count > 0)
            {
                _log.WriteLine($"[CONTENT] Loaded from '{path}':");

                ThreadingHelper.InterlockSpinWait(ref _lockerVal, () =>
                {
                    foreach (Type t in e.Result.Objects.Keys)
                    {
                        ContentSegment contentFile = GetContentFile(e.FilePathString, e.ContentType);
                        List<object> result = e.Result.Objects[t];
                        contentFile.Objects.AddRange(result);
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

        public string RootDirectory
        {
            get => _rootDirectory;
            set => _rootDirectory = value;
        }

        internal Logger Log => _log;
    }
}
