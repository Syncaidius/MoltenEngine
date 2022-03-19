using Molten.Collections;
using System;
using System.Collections.Generic;
using System.IO;

namespace Molten
{
    public class ContentContext : IPoolable
    {
        internal ContentContext() { }

        void IPoolable.ClearForPool()
        {
            Clear();
        }

        internal void Clear()
        {
            _info = null;
            Filename = string.Empty;
            Parameters = null;
            ContentType = null;
            Log = null;
            Engine = null;
            Output.Clear();
            Input.Clear();
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

        internal ContentFileParameters Parameters { get; set; }

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
