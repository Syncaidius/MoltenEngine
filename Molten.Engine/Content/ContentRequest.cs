using Molten.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public delegate void ContentRequestHandler(ContentManager content, ContentRequest cr);

    public class ContentRequest : IPoolable
    {
        public event ContentRequestHandler OnCompleted;

        public List<string> RequestedFiles { get; private set; } = new List<string>();

        internal List<ContentRequestElement> Elements = new List<ContentRequestElement>();

        internal ContentManager Manager;

        internal bool Cancelled;

        internal ContentRequest() { }

        /// <summary>Commits the request to it's parent content manager. It will be queued for processing as soon as possible.</summary>
        public void Commit()
        {
            Manager.Commit(this);
        }

        public void Cancel()
        {
            Cancelled = true;
        }

        internal void Complete()
        {
            OnCompleted?.Invoke(Manager, this);
        }

        /// <summary>Adds file load operation to the current <see cref="ContentRequest"/>.</summary>
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
                e.Result.AddResult<T>(obj);
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
                e.Result.AddResult<T>(obj);
            });
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="fn">The relative file path from the content manager's root directory.</param>
        public void Delete(string fn)
        {
            AddElement(fn, ContentRequestType.Delete, null);
        }

        private void AddElement(string requestString, ContentRequestType type, Type contentType, Action<ContentRequestElement> populator = null)
        {
            ContentRequestElement e = ContentManager.ElementPool.GetInstance();
            string path = Manager.ParseRequestString(requestString, e.Metadata);
            string contentPath = Path.Combine(Manager.RootDirectory, path);

            if (type == ContentRequestType.Read || type == ContentRequestType.Deserialize)
            {
                if (!File.Exists(contentPath))
                {
                    Manager.Log.WriteError($"[ERROR] Requested content file '{requestString}' does not exist");
                    return;
                }

                RequestedFiles.Add(path);
            }
            
            e.ContentType = contentType;
            e.FileRequestString = path;
            e.Type = type;
            e.Info = new FileInfo(contentPath);

            populator?.Invoke(e);
            Elements.Add(e);
        }

        public void Clear()
        {
            Manager = null;
            Elements.Clear();
            RequestedFiles.Clear();
        }
    }
}
