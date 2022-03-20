using Molten.Collections;

namespace Molten
{
    internal class ContentFile : IDisposable
    {
        FileInfo _file;

        ThreadedDictionary<Type, ThreadedList<object>> _segments = new ThreadedDictionary<Type, ThreadedList<object>>();

        internal Type[] GetTypeArray()
        {
            return _segments.Keys.ToArray();
        }

        public void Dispose()
        {
            ICollection<ThreadedList<object>> lists = _segments.Values;
            foreach (ThreadedList<object> list in lists)
            {
                foreach (object obj in list)
                {
                    if (obj is IDisposable disposable)
                        disposable.Dispose();
                }
            }
        }

        internal void AddObject(Type t, object obj)
        {
            ThreadedList<object> list;
            if (!_segments.TryGetValue(t, out list))
            {
                list = new ThreadedList<object>();
                _segments.Add(t, list);
            }

            list.Add(obj);
        }

        internal void AddObjects(Type t, IList<object> objects)
        {
            ThreadedList<object> list;
            if (!_segments.TryGetValue(t, out list))
            {
                list = new ThreadedList<object>();
                _segments.Add(t, list);
            }

            list.AddRange(objects);
        }

        internal IList<object> GetObjects(Type t)
        {
            return _segments[t];
        }

        internal object GetObject(Engine engine, Type t, ContentFileParameters parameters)
        {
            if (_segments.TryGetValue(t, out ThreadedList<object> list))
            {
                if (OriginalRequestType == ContentRequestType.Read)
                {
                    return OriginalProcessor.OnGet(engine, t, parameters, list);
                }
                else if (OriginalRequestType == ContentRequestType.Deserialize)
                {
                    return list[0];
                }
            }

            return default;
        }

        internal FileInfo File
        {
            get => _file;
            set
            {
                _file = value;
                Path = _file?.ToString() ?? "";
            }
        }

        internal string Path { get; private set; }

        internal ContentRequestType OriginalRequestType { get; set; }

        /// <summary>
        /// The content processor which loaded the file, if any.
        /// </summary>
        internal ContentProcessor OriginalProcessor { get; set; }

        /// <summary>
        /// The content type that was requested when loading the file.
        /// </summary>
        internal Type OriginalContentType { get; set; }

        /// <summary>
        /// Gets the parameters that the file was originally loaded with.
        /// </summary>
        internal ContentFileParameters Parameters { get;set; }
    }
}
