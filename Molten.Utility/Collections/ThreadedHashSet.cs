using System.Collections;
using System.Collections.Concurrent;

namespace Molten.Utility.Collections
{
    /// <summary>Wraps a .NET core <see cref="HashSet{T}"/> in thread-safe, lock-free interlock code.</summary>
    /// <typeparam name="T"></typeparam>
    public partial class ThreadedHashSet<T> : ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IProducerConsumerCollection<T>
    {
        HashSet<T> _set;
        Interlocker _interlocker;

        public ThreadedHashSet()
            : this(EqualityComparer<T>.Default) { }

        public ThreadedHashSet(IEnumerable<T> collection)
            : this(collection, EqualityComparer<T>.Default) { }

        /// <summary>
        /// Implementation Notes:
        /// Since resizes are relatively expensive (require rehashing), this attempts to minimize 
        /// the need to resize by setting the initial capacity based on size of collection. 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="comparer"></param>
        public ThreadedHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
            : this(comparer)
        {
            _set = new HashSet<T>(collection, comparer);
            _interlocker = new Interlocker();
        }

        public ThreadedHashSet(IEqualityComparer<T> comparer)
        {
            _set = new HashSet<T>(comparer);
            _interlocker = new Interlocker();
        }

        /// <summary>
        /// Adds a new item to the hashset.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns></returns>
        public bool Add(T item)
        {
            bool result = false;
            _interlocker.Lock(() => result = _set.Add(item));
            return result;
        }

        /// <summary>
        /// Clears the hashset.
        /// </summary>
        public void Clear()
        {
            _interlocker.Lock(() => _set.Clear());
        }

        /// <summary>
        /// Returns true if the hashset contains the specified item, or false if it does not.
        /// </summary>
        /// <param name="item">The item to be checked.</param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            bool result = false;
            _interlocker.Lock(() => result = _set.Contains(item));
            return result;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _interlocker.Lock(() => _set.CopyTo(array, arrayIndex));
        }

        void ICollection.CopyTo(Array array, int index)
        {
            _interlocker.Lock(() =>
            {
                T[] src = array as T[];
                if (src == null)
                    throw new Exception("Incompatible array was provided.");

                _set.CopyTo(src, index);
            });
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("The other enumerable cannot be null.");

            _interlocker.Lock(() => _set.ExceptWith(other));
        }

        public IEnumerator<T> GetEnumerator()
        {
            IEnumerator<T> enumerator = null;
            _interlocker.Lock(() => enumerator = _set.GetEnumerator());
            return enumerator;
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            _interlocker.Lock(() => _set.IntersectWith(other));
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            bool result = false;
            _interlocker.Lock(() => result = _set.IsProperSubsetOf(other));
            return result;
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            bool result = false;
            _interlocker.Lock(() => result = _set.IsProperSupersetOf(other));
            return result;
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            bool result = false;
            _interlocker.Lock(() => result = _set.IsSubsetOf(other));
            return result;
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            bool result = false;
            _interlocker.Lock(() => result = _set.IsSupersetOf(other));
            return result;
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            bool result = false;
            _interlocker.Lock(() => result = _set.Overlaps(other));
            return result;
        }

        public bool Remove(T item)
        {
            bool result = false;
            _interlocker.Lock(() => result = _set.Remove(item));
            return result;
        }

        public int RemoveWhere(Predicate<T> match)
        {
            int result = 0;
            _interlocker.Lock(() => result = _set.RemoveWhere(match));
            return result;
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            bool result = false;
            _interlocker.Lock(() => result = _set.SetEquals(other));
            return result;
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            _interlocker.Lock(() => _set.SymmetricExceptWith(other));
        }

        public void UnionWith(IEnumerable<T> other)
        {
            _interlocker.Lock(() => _set.UnionWith(other));
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void TrimExcess()
        {
            _interlocker.Lock(() => _set.TrimExcess());
        }

        public bool TryAdd(T item)
        {
            bool result = false;
            _interlocker.Lock(() => result = _set.Add(item));
            return result;
        }

        public bool TryTake(out T item)
        {
            bool result = false;
            T temp = default;

            _interlocker.Lock(() =>
            {
                if (_set.Count > 0)
                {
                    temp = _set.Last();
                    result = _set.Remove(temp);
                }
                else
                {
                    temp = default;
                }

            });
            item = temp;
            return result;
        }

        public T[] ToArray()
        {
            T[] array = null;
            _interlocker.Lock(() => array = _set.ToArray());
            return array;
        }

        public int Count => _set.Count;

        public bool IsReadOnly => false;

        object ICollection.SyncRoot => throw new NotSupportedException("ThreadedHashSet does not require a sync root object.");

        public bool IsSynchronized => true;

        /// <summary>Gets the <see cref="EqualityComparer{T}"/> object that is used to
        /// determine equality for the values in the set.</summary>
        public IEqualityComparer<T> Comparer => _set.Comparer;
    }
}
