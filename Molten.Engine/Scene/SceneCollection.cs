using Molten.Collections;
using System.Collections;

namespace Molten
{
    public class SceneCollection<T> : IEnumerable<T>, ICollection<T>
        where T : class
    {
        public struct EventData
        {
            bool _cancel;

            /// <summary>
            /// Gets or sets whether the operation should be cancelled. If true, the <see cref="SceneCollection{T}"/> will not proceed with the operation.
            /// </summary>
            public bool Cancel
            {
                get => _cancel;
                set => _cancel = _cancel || value;
            }
        }

        public delegate void Handler(SceneCollection<T> collection, T obj);

        public delegate void DataHandler(SceneCollection<T> collection, T obj, ref EventData data);

        internal ThreadedList<T> Objects { get; }

        public event DataHandler OnAdd;

        /// <summary>
        /// Occurs when an object of type T is added to the collection.
        /// </summary>
        public event Handler OnAdded;

        public event DataHandler OnRemove;

        /// <summary>
        /// Occurs when an object of type T is removed from the collection.
        /// </summary>
        public event Handler OnRemoved;

        internal SceneCollection(SceneObject parent)
        {
            Objects = new ThreadedList<T>();
            Parent = parent;
        }

        /// <summary>
        /// Adds a <see cref="SceneObject"/> to the collection.
        /// </summary>
        /// <param name="obj">The object to add.</param>
        public void Add(T obj)
        {
            EventData data = new EventData();
            Interlocked.CompareExchange(ref OnAdd, null, null)?.Invoke(this, obj, ref data);

            if (!data.Cancel)
            {
                Objects.Add(obj);
                Interlocked.CompareExchange(ref OnAdded, null, null)?.Invoke(this, obj);
            }
        }

        /// <summary>
        /// Removes a <see cref="SceneObject"/> from the collection.
        /// </summary>
        /// <param name="obj">the object to remove.</param>
        /// <returns>Returns a true if the object was successfully removed, or false if removal was unsuccessful or cancelled. </returns>
        public bool Remove(T obj)
        {
            EventData data = new EventData();
            Interlocked.CompareExchange(ref OnRemove, null, null)?.Invoke(this, obj, ref data);

            if (!data.Cancel)
            {
                bool result = Objects.Remove(obj);
                Interlocked.CompareExchange(ref OnRemoved, null, null)?.Invoke(this, obj);
                return result;
            }

            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Objects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Objects.GetEnumerator();
        }

        public void Clear()
        {
            // Clear before raising the OnRemoved event.
            T[] removed = Objects.ToArray();
            Objects.Clear();
            for (int i = 0; i < removed.Length; i++)
                Interlocked.CompareExchange(ref OnRemoved, null, null)?.Invoke(this, removed[i]);
        }

        public bool Contains(T item)
        {
            return Objects.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Objects.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets or sets a <see cref="SceneObject"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public T this[int index]
        {
            get => Objects[index];
            set
            {
                T old = Objects[index];
                Objects[index] = value;

                if (old != value)
                    Interlocked.CompareExchange(ref OnRemoved, null, null)?.Invoke(this, old);

                Interlocked.CompareExchange(ref OnAdded, null, null)?.Invoke(this, value);
            }
        }

        /// <summary>
        /// Gets the number of objects in the collection.
        /// </summary>
        public int Count => Objects.Count;

        /// <summary>
        /// Gets whether or not the collection is readonly.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets the <see cref="SceneObject"/> that the current <see cref="SceneCollection{T}"/> is bound to.
        /// </summary>
        public SceneObject Parent { get; }
    }
}
