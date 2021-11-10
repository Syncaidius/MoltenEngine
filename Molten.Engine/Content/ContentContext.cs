using Molten.Collections;
using System;
using System.Collections.Generic;
using System.IO;

namespace Molten
{
    public class ContentContext : IPoolable
    {
        Dictionary<string, object> _contextData = new Dictionary<string, object>();

        internal ContentContext() { }

        void IPoolable.Clear()
        {
            Clear();
        }

        internal void Clear()
        {
            _info = null;
            Filename = string.Empty;
            Metadata.Clear();
            ContentType = null;
            Log = null;
            Engine = null;
            Output.Clear();
            Input.Clear();
            _contextData.Clear();
        }

        public void SetContextData<T>(string key, T obj)
        {
            _contextData[key] = obj;
        }

        public T GetContextData<T>(string key)
        {
            if (!_contextData.TryGetValue(key, out object obj))
                return default;
            else
                return (T)obj;
        }

        public void AddOutput<T>(T obj)
        {
            if (obj == null)
                return;

            Type t = typeof(T);
            AddResult(Output, t, obj);
        }

        public void AddOutput(Type t, object obj)
        {
            AddResult(Output, t, obj);
        }

        internal void AddInput(Type t, object obj)
        {
            AddResult(Input, t, obj);
        }

        private void AddResult(Dictionary<Type, List<object>> dest, Type t, object obj)
        {
            List<object> group = null;
            if (!dest.TryGetValue(t, out group))
            {
                group = new List<object>();
                dest.Add(t, group);
            }

            group.Add(obj);
        }

        internal ContentRequestType RequestType { get; set; }

        public Engine Engine { get; internal set; }

        public Logger Log { get; internal set; }

        public Type ContentType { get; internal set; }

        public Dictionary<string, string> Metadata { get; internal set; } = new Dictionary<string, string>();

        public FileInfo File
        {
            get => _info;
            internal set
            {
                _info = value;
                Filename = _info.ToString();
            }
        }

        internal Dictionary<Type, List<object>> Output = new Dictionary<Type, List<object>>();

        internal Dictionary<Type, List<object>> Input = new Dictionary<Type, List<object>>();

        public string Filename { get; private set; }

        FileInfo _info;
    }
}
