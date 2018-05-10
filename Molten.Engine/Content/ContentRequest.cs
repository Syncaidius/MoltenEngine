using Molten.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public delegate void ContentRequestHandler(ContentManager manager, ContentRequest request);
    
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

        internal ContentRequestState State;

        List<string> _requestedFiles = new List<string>();

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
            OnCompleted?.Invoke(Manager, this);
        }

        /// <summary>Adds file load operation to the current <see cref="ContentRequest"/>. </para>
        /// If the content was already loaded from a previous request, the existing object will be retrieved.</summary>
        /// <param name="fn">The relative file path from the content manager's root directory.</param>
        public void Load<T>(string fn)
        {
            AddElement(fn, ContentRequestType.Read, typeof(T));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fn">The relative file path from the content manager's root directory.</param>
        /// <param name="obj">The object to be written.</param>
        public void Write<T>(string fn, T obj)
        {
            AddElement(fn, ContentRequestType.Write, typeof(T), (e) =>
            {
                e.AddInput<T>(obj);
            });
        }

        /// <summary>Adds a deserialize operation to the current <see cref="ContentRequest"/>. This will deserialize an object from the specified JSON file.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fn"></param>
        public void Deserialize<T>(string fn)
        {
            AddElement(fn, ContentRequestType.Deserialize, typeof(T));
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
            string contentPath = Path.Combine(Manager.RootDirectory, path);

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
            c.Type = type;
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
            OnCompleted = null;
        }

        /// <summary>
        /// Gets a requested content file path at the specified index.
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
