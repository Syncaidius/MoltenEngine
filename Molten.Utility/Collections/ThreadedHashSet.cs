using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Molten.Utility.Collections
{
    /// <summary>Wraps a .NET core <see cref="HashSet{T}"/> in thread-safe, lock-free interlock code.</summary>
    /// <typeparam name="T"></typeparam>
    public partial class ThreadedHashSet<T> : ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IProducerConsumerCollection<T>
    {
        HashSet<T> _set;
        int _blockingVal;

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
            : this(comparer) {
            _set = new HashSet<T>(collection, comparer);
        }

        public ThreadedHashSet(IEqualityComparer<T> comparer)
        {
            _set = new HashSet<T>(comparer);
        }

        public bool Add(T item)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    bool result = _set.Add(item);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return result;
                }
                spin.SpinOnce();
            }
        }

        public void Clear()
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    _set.Clear();
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        public bool Contains(T item)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    bool result = _set.Contains(item);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return result;
                }
                spin.SpinOnce();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    _set.CopyTo(array, arrayIndex);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        public void CopyTo(Array array, int index)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    T[] src = array as T[];
                    if (src == null)
                        throw new Exception("Incompatible array was provided.");

                    _set.CopyTo(src, index);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    _set.ExceptWith(other);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    IEnumerator<T> e = _set.GetEnumerator();
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return e;
                }
                spin.SpinOnce();
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    _set.IntersectWith(other);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    bool result = _set.IsProperSubsetOf(other);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return result;
                }
                spin.SpinOnce();
            }
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    bool result = _set.IsProperSupersetOf(other);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return result;
                }
                spin.SpinOnce();
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    bool result = _set.IsSubsetOf(other);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return result;
                }
                spin.SpinOnce();
            }
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    bool result = _set.IsSupersetOf(other);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return result;
                }
                spin.SpinOnce();
            }
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    bool result = _set.Overlaps(other);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return result;
                }
                spin.SpinOnce();
            }
        }

        public bool Remove(T item)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    bool result = _set.Remove(item);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return result;
                }
                spin.SpinOnce();
            }
        }

        public int RemoveWhere(Predicate<T> match)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    int result = _set.RemoveWhere(match);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return result;
                }
                spin.SpinOnce();
            }
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    bool result = _set.SetEquals(other);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return result;
                }
                spin.SpinOnce();
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    _set.SymmetricExceptWith(other);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    _set.UnionWith(other);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
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
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    _set.TrimExcess();
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        public bool TryAdd(T item)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    bool result = _set.Add(item);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return result;
                }
                spin.SpinOnce();
            }
        }

        public bool TryTake(out T item)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    bool success = false;

                    if (_set.Count > 0)
                    {
                        item = _set.Last();
                        success = _set.Remove(item);
                    }
                    else
                    {
                        item = default(T);
                    }

                    Interlocked.Exchange(ref _blockingVal, 0);
                    return success;
                }
                spin.SpinOnce();
            }
        }

        public T[] ToArray()
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    T[] array = _set.ToArray();
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return array;
                }
                spin.SpinOnce();
            }
        }

        public int Count
        {
            get { return _set.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get
            {
                throw new NotSupportedException("ThreadedHashSet does not require a sync root object.");
            }
        }

        public bool IsSynchronized
        {
            get { return true; }
        }

        /// <summary>Gets the <see cref="EqualityComparer{T}"/> object that is used to
        /// determine equality for the values in the set.</summary>
        public IEqualityComparer<T> Comparer
        {
            get { return _set.Comparer; }
        }
    }
}
