using Molten.Collections;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

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
        static char[] REQUEST_SPLITTER = new char[] { ';' };
        static char[] METADATA_ASSIGNMENT = new char[] { '=' };

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

        /// <summary>Adds file load operation to the current <see cref="ContentRequest"/>. </para>
        /// If the content was already loaded from a previous request, the existing object will be retrieved.</summary>
        /// <param name="fn">The relative file path from the request's root directory.</param>
        public void Load<T>(string fn)
        {
            AddElement(fn, ContentRequestType.Read, typeof(T));
        }

        /// <summary>Adds file load operation to the current <see cref="ContentRequest"/>. </para>
        /// If the content was already loaded from a previous request, the existing object will be retrieved.</summary>
        /// <param name="fn">The relative file path from the request's root directory.</param>
        public void Load(Type t, string fn)
        {
            AddElement(fn, ContentRequestType.Read, t);
        }

        /// <summary>Adds a write request for the provided object.</summary>
        /// <param name="fn">The relative file path from the request's root directory.</param>
        /// <param name="obj">The object to be written.</param>
        public void Save<T>(string fn, T obj)
        {
            Save(typeof(T), fn, obj);
        }

        /// <summary>Adds a write request for the provided object.</summary>
        /// <param name="fn">The relative file path from the request's root directory.</param>
        /// <param name="obj">The object to be written.</param>
        public void Save(Type type, string fn, object obj)
        {
            AddElement(fn, ContentRequestType.Write, type, (e) =>
            {
                e.AddInput(type, obj);
            });
        }

        /// <summary>Adds a write request for the provided object.</summary>
        /// <param name="fn">The relative file path from the request's root directory.</param>
        /// <param name="obj">The object to be written.</param>
        public void Save<T>(string fn, params T[] obj)
        {
            Save(typeof(T), fn, obj);
        }
        /// <summary>Adds a write request for the provided object.</summary>
        /// <param name="fn">The relative file path from the request's root directory.</param>
        /// <param name="obj">The object to be written.</param>
        public void Save(Type type, string fn, params object[] obj)
        {
            if (obj == null)
                return;

            AddElement(fn, ContentRequestType.Write, type, (e) =>
            {
                for (int i = 0; i < obj.Length; i++)
                    e.AddInput(type, obj[i]);
            });
        }

        /// <summary>Adds a deserialize operation to the current <see cref="ContentRequest"/>. This will deserialize an object from the specified JSON file.</summary>
        /// <typeparam name="T">The type of object to be deserialized.</typeparam>
        /// <param name="fn">The relative file path from the request's root directory.</param>
        public void Deserialize<T>(string fn)
        {
            AddElement(fn, ContentRequestType.Deserialize, typeof(T));
        }

        /// <summary>Adds a deserialize operation to the current <see cref="ContentRequest"/>. This will deserialize an object from the specified JSON file.</summary>
        /// <param name="type">The type of object to be deserialized.</typeparam>
        /// <param name="fn">The file name and path.</param>
        public void Deserialize(Type type, string fn)
        {
            AddElement(fn, ContentRequestType.Deserialize, type);
        }

        /// <summary>Adds a serialization operation to the current <see cref="ContentRequest"/>. This will serialize an object into JSON and write it to the specified file.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fn"></param>
        /// <param name="obj"></param>
        public void Serialize<T>(string fn, T obj)
        {
            Serialize(typeof(T), fn, obj);
        }

        /// <summary>Adds a serialization operation to the current <see cref="ContentRequest"/>. This will serialize an object into JSON and write it to the specified file.</summary>
        /// <param name="fn"></param>
        /// <param name="obj"></param>
        /// <param name="type">The type of object to be serialized.</param>
        public void Serialize(Type type, string fn, object obj)
        {
            AddElement(fn, ContentRequestType.Serialize, type, (e) =>
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
            AddElement(fn, ContentRequestType.Delete, null);
        }

        private void AddElement(string requestString, ContentRequestType type, Type contentType, Action<ContentContext> populator = null)
        {
            ContentContext c = ContentManager.ContextPool.GetInstance();
            string path = ParseRequestString(Manager.Log, requestString, c.Metadata);
            string contentPath = Path.Combine(RootDirectory, path);

            if (type == ContentRequestType.Read || type == ContentRequestType.Deserialize)
                _requestedFiles.Add(path);

            c.ContentType = contentType;
            c.RequestType = type;
            c.File = new FileInfo(contentPath);

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
        /// <param name="requestString">The path or request string.</param>
        /// <returns></returns>
        public T Get<T>(string requestString)
        {
            return (T)Get(typeof(T), requestString);
        }

        /// <summary>
        /// Returns a content object that was loaded or retrieved as part of the request.
        /// </summary>
        /// <typeparam name="T">The type of object expected to be returned.</typeparam>
        /// <param name="requestString">The path or request string.</param>
        /// <returns></returns>
        public object Get(Type type, string requestString)
        {
            Dictionary<string, string> meta = new Dictionary<string, string>();
            string path = Path.Combine(RootDirectory, ParseRequestString(Manager.Log, requestString, meta));
            path = path.ToLower();

            if (RetrievedContent.TryGetValue(path, out ContentFile file))
                return file.GetObject(Manager.Engine, type, meta);

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

        internal static string ParseRequestString(Logger log, string requestString, Dictionary<string, string> metadataOut)
        {
            string[] parts = requestString.Split(REQUEST_SPLITTER, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return requestString;

            string path = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string[] metaParts = parts[i].Split(METADATA_ASSIGNMENT, StringSplitOptions.RemoveEmptyEntries);
                if (metaParts.Length != 2)
                {
                    log.Error($"Invalid metadata segment in content request: {parts[i]}");
                    continue;
                }
                metadataOut.Add(metaParts[0], metaParts[1]);
            }

            return path;
        }



        /// <summary>
        /// Gets the path of a file that was loaded via the current content request.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string this[int index]
        {
            get => _requestedFiles[index];
        }

        /// <summary>
        /// Gets the number of files that were requested
        /// </summary>
        public int RequestedFileCount => _requestedFiles.Count;
    }
}
