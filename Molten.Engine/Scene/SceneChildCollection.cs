using Molten.Collections;
using System.Collections;

namespace Molten
{
    public delegate void SceneChildCollectionHandler(SceneChildCollection collection, SceneObject obj);

    public class SceneChildCollection : IEnumerable<SceneObject>, ICollection<SceneObject>
    {
        internal readonly ThreadedList<SceneObject> Objects;
        readonly SceneObject _parent;

        /// <summary>
        /// Occurs when a <see cref="SceneObject"/> is added to the collection.
        /// </summary>
        public event SceneChildCollectionHandler OnAdded;

        /// <summary>
        /// Occurs when a <see cref="SceneObject"/> is removed from the collection.
        /// </summary>
        public event SceneChildCollectionHandler OnRemoved;

        internal SceneChildCollection(SceneObject parent)
        {
            _parent = parent;
            Objects = new ThreadedList<SceneObject>();
        }

        /// <summary>
        /// Adds a <see cref="SceneObject"/> to the collection.
        /// </summary>
        /// <param name="obj">The object to add.</param>
        public void Add(SceneObject obj)
        {
            Objects.Add(obj);
            Interlocked.CompareExchange(ref OnAdded, null, null)?.Invoke(this, obj);
        }

        /// <summary>
        /// Removes a <see cref="SceneObject"/> from the collection.
        /// </summary>
        /// <param name="obj">the object to remove.</param>
        public bool Remove(SceneObject obj)
        {
            bool result = Objects.Remove(obj);
            Interlocked.CompareExchange(ref OnRemoved, null, null)?.Invoke(this, obj);
            return result;
        }

        public IEnumerator<SceneObject> GetEnumerator()
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
            SceneObject[] removed = Objects.ToArray();
            Objects.Clear();
            for (int i = 0; i < removed.Length; i++)
                Interlocked.CompareExchange(ref OnRemoved, null, null)?.Invoke(this, removed[i]);

            removed = null;
        }

        public bool Contains(SceneObject item)
        {
            return Objects.Contains(item);
        }

        public void CopyTo(SceneObject[] array, int arrayIndex)
        {
            Objects.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets or sets a <see cref="SceneObject"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public SceneObject this[int index]
        {
            get => Objects[index];
            set
            {
                SceneObject old = Objects[index];
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
        /// Gets the parent object which owns the current <see cref="SceneChildCollection"/>.
        /// </summary>
        public SceneObject Object => _parent;

        /// <summary>
        /// Gets whether or not the collection is readonly.
        /// </summary>
        public bool IsReadOnly => false;
    }
}