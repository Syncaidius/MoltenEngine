using Molten.Collections;
using Newtonsoft.Json;

namespace Molten
{
    public delegate void ContentRequestHandler(ContentRequest request);

    public enum ContentRequestState
    {
        NotCommited = 0,

        Committed = 1,

        Completed = 2,
    }

    public class ContentRequest : IPoolable
    {
        public event ContentRequestHandler OnCompleted;

        internal List<ContentContext> RequestElements = new List<ContentContext>();

        internal ContentManager Manager;

        internal JsonSerializerSettings JsonSettings;

        internal string RootDirectory;

        internal ContentRequestState State;

        List<string> _requestedFiles = new List<string>();

        internal Dictionary<string, ContentFile> RetrievedContent = new Dictionary<string, ContentFile>();

        internal ContentRequest() { }

        /// <summary>Commits and immediately processes the <see cref="ContentRequest"/> on the calling thread.</summary>
        public void CommitImmediate()
        {
            if (State != ContentRequestState.NotCommited)
                throw new ContentException("Content request has already been committed.");

            State = ContentRequestState.Committed;
            Manager.CommitImmediate(this);
        }

        /// <summary>Commits the request to it's parent content manager. It will be queued for processing as soon as possible.</summary>
        public void Commit()
        {
            if (State != ContentRequestState.NotCommited)
                throw new ContentException("Content request has already been committed.");

            State = ContentRequestState.Committed;
            Manager.Commit(this);
        }

        internal void Complete()
        {
            State = ContentRequestState.Completed;
            OnCompleted?.Invoke(this);
        }

        /// <summary>Adds file load operation to the current <see cref="ContentRequest"/>.
        /// If the content was already loaded from a previous request, the existing object will be retrieved.</summary>
        /// <param name="fn">The relative file path from the request's root directory.</param>
        /// <param name="parameters">A list of parameters to use when loading the asset.</param>
        public void Load<T>(string fn, IContentParameters parameters = null)
        {
            AddElement(fn, ContentRequestType.Read, typeof(T), parameters);
        }

        /// <summary>Adds file load operation to the current <see cref="ContentRequest"/>.
        /// If the content was already loaded from a previous request, the existing object will be retrieved.</summary>
        /// <param name="fn">The relative file path from the request's root directory.</param>
        /// <param name="parameters">A set of to use when loading the asset.</param>
        public void Load(Type t, string fn, IContentParameters parameters = null)
        {
            AddElement(fn, ContentRequestType.Read, t, parameters);
        }

        /// <summary>Adds a write request for the provided object.</summary>
        /// <param name="fn">The relative file path from the request's root directory.</param>
        /// <param name="obj">The object to be written.</param>
        public void Save<T>(string fn, T obj, IContentParameters parameters = null)
        {
            Save(typeof(T), fn, obj, parameters);
        }

        /// <summary>Adds a write request for the provided object.</summary>
        /// <param name="fn">The relative file path from the request's root directory.</param>
        /// <param name="obj">The object to be written.</param>
        public void Save(Type type, string fn, object obj, IContentParameters parameters = null)
        {
            AddElement(fn, ContentRequestType.Write, type, parameters, (e) => e.AddInput(type, obj));
        }

        /// <summary>Adds a deserialize operation to the current <see cref="ContentRequest"/>. This will deserialize an object from the specified JSON file.</summary>
        /// <typeparam name="T">The type of object to be deserialized.</typeparam>
        /// <param name="fn">The relative file path from the request's root directory.</param>
        public void Deserialize<T>(string fn, IContentParameters parameters = null)
        {
            AddElement(fn, ContentRequestType.Deserialize, typeof(T), parameters);
        }

        /// <summary>Adds a deserialize operation to the current <see cref="ContentRequest"/>. This will deserialize an object from the specified JSON file.</summary>
        /// <param name="type">The type of object to be deserialized.</typeparam>
        /// <param name="fn">The file name and path.</param>
        public void Deserialize(Type type, string fn, IContentParameters parameters = null)
        {
            AddElement(fn, ContentRequestType.Deserialize, type, parameters);
        }

        /// <summary>Adds a serialization operation to the current <see cref="ContentRequest"/>. This will serialize an object into JSON and write it to the specified file.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fn">The filename.</param>
        /// <param name="obj">The object to be serialized.</param>
        public void Serialize<T>(string fn, T obj, IContentParameters parameters = null)
        {
            Serialize(typeof(T), fn, obj, parameters);
        }

        /// <summary>Adds a serialization operation to the current <see cref="ContentRequest"/>. This will serialize an object into JSON and write it to the specified file.</summary>
        /// <param name="fn">The filename.</param>
        /// <param name="obj">The object to be serialized.</param>
        /// <param name="type">The type of object to be serialized.</param>
        public void Serialize(Type type, string fn, object obj, IContentParameters parameters = null)
        {
            AddElement(fn, ContentRequestType.Serialize, type, parameters, (e) =>
            {
                e.AddInput(type, obj);
            });
        }

        /// <summary>
        /// Requests the deletion of a file.
        /// </summary>
        /// <param name="fn">The relative file path from the content manager's root directory.</param>
        public void Delete(string fn)
        {
            AddElement(fn, ContentRequestType.Delete, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="contentType"></param>
        /// <param name="parameters"></param>
        /// <param name="populator">A callback method for populating various elements of a <see cref="ContentContext"/>.</param>
        private void AddElement(string path, ContentRequestType type, Type contentType, IContentParameters parameters, Action<ContentContext> populator = null)
        {
            ContentContext c = Manager.ContextPool.GetInstance();
            string contentPath = Path.Combine(RootDirectory, path);

            if (type == ContentRequestType.Read || type == ContentRequestType.Deserialize)
                _requestedFiles.Add(path);

            c.ContentType = contentType;
            c.RequestType = type;
            c.File = new FileInfo(contentPath);
            c.Parameters = parameters;

            populator?.Invoke(c);
            RequestElements.Add(c);
        }

        public void ClearForPool()
        {
            State = ContentRequestState.NotCommited;
            Manager = null;
            RequestElements.Clear();
            _requestedFiles.Clear();
            RetrievedContent.Clear();
            OnCompleted = null;
            RootDirectory = null;
        }

        /// <summary>
        /// Returns a content object that was loaded or retrieved as part of the request.
        /// </summary>
        /// <typeparam name="T">The type of object expected to be returned.</typeparam>
        /// <param name="path">The asset path.</param>
        /// <returns></returns>
        public T Get<T>(string path)
        {
            return (T)Get(typeof(T), path);
        }

        /// <summary>
        /// Returns a content object that was loaded or retrieved as part of the request.
        /// </summary>
        /// <typeparam name="T">The type of object expected to be returned.</typeparam>
        /// <param name="path">The asset path.</param>
        /// <returns></returns>
        public object Get(Type type, string path)
        {
            Dictionary<string, string> meta = new Dictionary<string, string>();
            path = Path.Combine(RootDirectory, path);
            path = path.ToLower();

            if (RetrievedContent.TryGetValue(path, out ContentFile file))
                return file.GetObject(Manager.Engine, type, file.Parameters);

            return default;
        }

        /// <summary>
        /// Returns a content object that was loaded or retrieved as part of the request.
        /// </summary>
        /// <typeparam name="T">The type of object expected to be returned.</typeparam>
        /// <param name="index">The request index of the object to be retrieved.</param>
        /// <returns></returns>
        public T Get<T>(int index)
        {
            return Get<T>(_requestedFiles[index]);
        }

        /// <summary>
        /// Returns a content object that was loaded or retrieved as part of the request.
        /// </summary>
        /// <param name="type">The type of object expected to be returned.</param>
        /// <param name="index">The request index of the object to be retrieved.</param>
        /// <returns></returns>
        public object Get(Type type, int index)
        {
            return Get(type, _requestedFiles[index]);
        }

        /// <summary>
        /// Gets the path of a file that was loaded via the current content request.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string this[int index] => _requestedFiles[index];

        /// <summary>
        /// Gets the number of files that were requested
        /// </summary>
        public int RequestedFileCount => _requestedFiles.Count;
    }
}
