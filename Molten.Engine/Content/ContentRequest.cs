using Molten.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public delegate void ContentRequestHandler(ContentRequest request);
    
    public enum ContentRequestState
    {
        NotCommited = 0,

        Committed = 1,

        Completed = 2,

        Cancelled = 3,
    }

    public class ContentRequest : IPoolable
    {
        public event ContentRequestHandler OnCompleted;

        internal List<ContentContext> RequestElements = new List<ContentContext>();

        internal ContentManager Manager;

        internal string RootDirectory;

        internal ContentRequestState State;

        List<string> _requestedFiles = new List<string>();

        internal Dictionary<string, ContentFile> RetrievedContent = new Dictionary<string, ContentFile>();

        internal ContentRequest() { }

        /// <summary>Commits the request to it's parent content manager. It will be queued for processing as soon as possible.</summary>
        public void Commit()
        {
            if (State != ContentRequestState.NotCommited)
                throw new ContentException("Content request has already been commited.");

            State = ContentRequestState.Committed;
            Manager.Commit(this);
        }

        public void Cancel()
        {
            if (State == ContentRequestState.Committed)
                State = ContentRequestState.Cancelled;
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
        public void Load(string fn, Type t)
        {
            AddElement(fn, ContentRequestType.Read, t);
        }

        /// <summary>Adds a write request for the provided object.</summary>
        /// <param name="fn">The relative file path from the request's root directory.</param>
        /// <param name="obj">The object to be written.</param>
        public void Write<T>(string fn, T obj)
        {
            AddElement(fn, ContentRequestType.Write, typeof(T), (e) =>
            {
                e.AddInput<T>(obj);
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
        /// <param name="t">The type of object to be deserialized.</typeparam>
        /// <param name="fn">The file name and path.</param>
        public void Deserialize(string fn, Type t)
        {
            AddElement(fn, ContentRequestType.Deserialize, t);
        }

        /// <summary>Adds a serialization operation to the current <see cref="ContentRequest"/>. This will serialize an object into JSON and write it to the specified file.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fn"></param>
        /// <param name="obj"></param>
        public void Serialize<T>(string fn, T obj)
        {
            AddElement(fn, ContentRequestType.Serialize, typeof(T), (e) =>
            {
                e.AddInput<T>(obj);
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
            string path = Manager.ParseRequestString(requestString, c.Metadata);
            string contentPath = Path.Combine(RootDirectory, path);

            if (type == ContentRequestType.Read || type == ContentRequestType.Deserialize)
            {
                if (!File.Exists(contentPath))
                {
                    Manager.Log.WriteError($"[ERROR] Requested content file '{requestString}' does not exist");
                    return;
                }

                _requestedFiles.Add(path);
            }
            
            c.ContentType = contentType;
            c.RequestType = type;
            c.File = new FileInfo(contentPath);

            populator?.Invoke(c);
            RequestElements.Add(c);
        }

        public void Clear()
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
            Dictionary<string, string> meta = new Dictionary<string, string>();
            string path = Path.Combine(RootDirectory, Manager.ParseRequestString(requestString, meta));
            path = path.ToLower();

            Type t = typeof(T);

            if (RetrievedContent.TryGetValue(path, out ContentFile file))
            {
                if (file.Segments.TryGetValue(t, out ContentSegment segment))
                {
                    // Since the content exists, we know there's either a custom or default processor for it.
                    ContentProcessor proc = Manager.GetProcessor(t);
                    if (proc != null)
                    {
                        T obj = (T)proc.OnGet(Manager.Engine, t, meta, segment.Objects);
                        return obj;
                    }
                    else
                    {
                        Manager.Log.WriteError($"[CONTENT] [GET] unable to fulfil content request from {path}: No content processor available for type {t} or any of its derivatives.");
                    }
                }
            }

            return default;
        }

        public T Get<T>(int index)
        {
            return Get<T>(_requestedFiles[index]);
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
