using System.Collections;
using System.Collections.Concurrent;

namespace Molten.Collections
{
    /// <summary>
    /// A thread-safe queue collection where items are added to the end and removed from the start. A first-in-first-out collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class ThreadedQueue<T> : IEnumerable<T>, ICollection, IEnumerable, IProducerConsumerCollection<T>
    {
        T[] _items;
        bool _isEmpty;
        int _count;
        int _next;
        int _queueStart; // The start of the queue, where items are dequeued.

        object _locker;
        Interlocker _interlocker;

        /// <summary>
        /// Creates a new instance of <see cref="ThreadedQueue{T}"/>, with the specified initial capacity. 
        /// <para>The default initial capacity is 20.</para>
        /// </summary>
        /// <param name="initialCapacity">The initial capacity of the queue. The default is 20.</param>
        public ThreadedQueue(int initialCapacity = 20)
        {
            _items = new T[initialCapacity];
            _locker = new object();
            _isEmpty = true;
            _interlocker = new Interlocker();
        }

        public IEnumerator GetEnumerator()
        {
            IEnumerator enumerator = null;
            _interlocker.Lock(() => enumerator = new Enumerator(this));
            return enumerator;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            IEnumerator<T> enumerator = null;
            _interlocker.Lock(() => enumerator = new Enumerator(this));
            return enumerator;
        }

        /// <summary>Clears all items from the queue. This will not release the memory consumed by the internal array. To do so, the queue must be disposed.</summary>
        public void Clear()
        {
            _interlocker.Lock(() =>
            {
                if (_next > _queueStart)
                {
                    for (int i = _queueStart; i < _next; i++)
                        _items[i] = default;
                }
                else
                {
                    for (int i = _queueStart; i < _items.Length; i++)
                        _items[i] = default;

                    for (int i = 0; i < _next; i++)
                        _items[i] = default;
                }
                _next = 0;
                _count = 0;
                _queueStart = 0;
            });
        }

        /// <summary>
        /// Copies the contents of the queue to the specified array, starting at the given array index.
        /// </summary>
        /// <param name="destination">The array to copy to.</param>
        /// <param name="startIndex">The start index within the <paramref name="destination"/> array.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void CopyTo(Array destination, int startIndex)
        {
            T[] dest = destination as T[];

            if (dest == null)
                throw new InvalidOperationException("The target array is not of the correct type");

            _interlocker.Lock(() => CopyToArray(dest, startIndex));
        }

        public void CopyTo(T[] array, int index)
        {
            _interlocker.Lock(() => CopyToArray(array, index));
        }

        public T[] ToArray()
        {
            T[] result = null;
            _interlocker.Lock(() =>
            {
                result = new T[_count];
                CopyToArray(result, 0);
            });

            return result;
        }

        /// <summary>Copies the queue into the provided array, cutting out any whitespace in the process.</summary>
        /// <param name="target">The destination array.</param>
        /// <param name="destIndex">The index in the target array to start copying to.</param>
        private void CopyToArray(T[] target, int destIndex)
        {
            if (target.Length - destIndex < _count)
                throw new IndexOutOfRangeException("The target array is not large enough to accept the copy operation");

            _interlocker.Lock(() =>
            {
                if (_next >= _queueStart)
                {
                    Array.Copy(_items, _queueStart, target, 0, _count);
                }
                else
                {
                    int endCount = _items.Length - _queueStart;
                    Array.Copy(_items, _queueStart, target, destIndex, endCount);

                    destIndex += endCount;
                    Array.Copy(_items, 0, target, destIndex, _next);
                }
            });
        }

        /// <summary>
        /// Gets the next item in the queue without dequeing it.
        /// </summary>
        /// <param name="item">The item output.</param>
        /// <returns>True if an item was successfully found in the queue.</returns>
        public bool TryPeek(out T item)
        {
            bool result = false;
            T temp = default;
            _interlocker.Lock(() =>
            {
                if (!_isEmpty)
                {
                    temp = _items[_queueStart];
                    result = true;
                }
                else
                {
                    temp = default;
                }
            });

            item = temp;
            return result;
        }

        /// <summary>
        /// Gets whether or not the next item in the queue is, or derived from, the given type.
        /// </summary>
        /// <typeparam name="N">The type.</typeparam>
        /// <returns></returns>
        public bool IsNext<N>()
        {
            bool result = false;
            _interlocker.Lock(() =>
            {
                if (!_isEmpty)
                    result = typeof(N).IsAssignableFrom(_items[_queueStart].GetType());
            });

            return result;
        }

        public void Enqueue(T item)
        {
            _interlocker.Lock(() => Insert(item));
        }

        public void EnqueueRange(IEnumerable<T> range)
        {
            _interlocker.Lock(() => InsertRange(range));
        }

        /// <summary>
        /// Tries to take the next item from the queue.
        /// </summary>
        /// <param name="item">The item output.</param>
        /// <returns>True if the item was successfully de-queued. Returns false if the queue was empty.</returns>
        public bool TryDequeue(out T item)
        {
            bool result = false;
            T temp = default;
            _interlocker.Lock(() =>
            {
                if (!_isEmpty)
                {
                    temp = GetItem();
                    result = true;
                }
                else
                {
                    temp = default;
                }
            });
            item = temp;
            return result;
        }

        /// <summary>
        /// Checks if the queue contains the specified item.
        /// </summary>
        /// <param name="item">The item to be checked.</param>
        /// <returns>True if the item exists in the queue.</returns>
        public bool Contains(T item)
        {
            bool result = false;
            _interlocker.Lock(() =>
            {
                if (item == null)
                {
                    // Check if iteration needs to wrap around.
                    if (_queueStart < _next)
                    {
                        for (int i = _queueStart; i < _next; i++)
                        {
                            if (_items[i] == null)
                            {
                                result = true;
                                return;
                            }
                        }
                    }
                    else
                    {
                        // from start of queue to end of array.
                        for (int i = _queueStart; i < _items.Length; i++)
                        {
                            if (_items[i] == null)
                            {
                                result = true;
                                return;
                            }
                        }

                        // From 0 to _next
                        for (int i = 0; i < _next; i++)
                        {
                            if (_items[i] == null)
                            {
                                result = true;
                                return;
                            }
                        }
                    }
                }
                else
                {
                    EqualityComparer<T> c = EqualityComparer<T>.Default;
                    if (_queueStart < _next)
                    {
                        for (int i = 0; i < _count; i++)
                        {
                            if (c.Equals(_items[i], item))
                            {
                                result = true;
                                return;
                            }
                        }
                    }
                    else
                    {
                        // from start of queue to end of array.
                        for (int i = _queueStart; i < _items.Length; i++)
                        {
                            if (c.Equals(_items[i], item))
                            {
                                result = true;
                                return;
                            }
                        }

                        // From 0 to _next
                        for (int i = 0; i < _next; i++)
                        {
                            if (c.Equals(_items[i], item))
                            {
                                result = true;
                                return;
                            }
                        }
                    }
                }
            });

            return result;
        }

        private void EnsureCapacity(int required)
        {
            // Check if enough space is left in the array to fit the required amount.
            int available = _items.Length - _count;
            if (available >= required)
                return;

            if (_next == _queueStart) // Both values are identical.
            {
                if (_count == 0) //Queue is empty. No items to copy, so resize array and reset counters.
                {
                    int additional = required;
                    int newLength = (_items.Length * 2) + additional;
                    Array.Resize(ref _items, newLength);
                    _queueStart = 0;
                    _next = 0;
                }
                else // Assume the queue is 100% full
                {
                    int additional = required;
                    int newLength = (_items.Length * 2) + additional;
                    T[] newArray = new T[newLength];

                    //Copy end of current queue, then the start. Two segments, into a single contiguous list of items.
                    int copyLen = _items.Length - _queueStart;
                    Array.Copy(_items, _queueStart, newArray, 0, copyLen);
                    Array.Copy(_items, 0, newArray, copyLen, _next);

                    // Update counters + array.
                    _queueStart = 0;
                    _next = _count; // Move to the end of the queued items.
                    _items = newArray;
                }
            }
            else if (_next > _queueStart) // _next is ahead of _queueStart
            {
                // Extend array ahead of _next, no counter updates needed.
                int additional = required - available;
                int newLength = (_items.Length * 2) + additional;
                Array.Resize(ref _items, newLength);
            }
            else //_next is behind (<) _queueStart 
            {
                int additional = required - available;
                int newLength = (_items.Length * 2) + additional;
                T[] newArray = new T[newLength];

                int copyLen = _items.Length - _queueStart;
                Array.Copy(_items, _queueStart, newArray, 0, copyLen);
                if (_next > 0)
                    Array.Copy(_items, 0, newArray, copyLen, _next);

                _queueStart = 0;
                _next = _count;
                _items = newArray;
            }
        }

        /// <summary>Pull the next item from the queue and return it.</summary>
        private T GetItem()
        {
            if (_queueStart == _items.Length)
                _queueStart = 0;

            T item = _items[_queueStart];

            _items[_queueStart] = default(T);
            _queueStart++;
            _count--;
            _isEmpty = _count == 0;

            return item;
        }

        private void Insert(T item)
        {
            EnsureCapacity(1);
            if (_next == _items.Length)
                _next = 0;

            _items[_next++] = item;

            _count++;
            _isEmpty = false;
        }

        private void InsertRange(IEnumerable<T> collection)
        {
            ICollection<T> c = collection as ICollection<T>;
            if (c != null)
            {
                if (c.Count > 0)
                {
                    EnsureCapacity(c.Count);

                    // Check if the next free ID is in front of the queue start/head ID.
                    if (_next >= _queueStart)
                    {
                        // Check if the collection can be copied in a single Array.Copy call.
                        int elementsAhead = _items.Length - _next;
                        if (elementsAhead >= c.Count)
                        {
                            c.CopyTo(_items, _next);
                            _next += c.Count;
                        }
                        else
                        {
                            // Create a temp array to copy the collection to.
                            T[] temp = new T[c.Count];
                            c.CopyTo(temp, 0);

                            if (elementsAhead > 0)
                                Array.Copy(temp, 0, _items, _next, elementsAhead);

                            // Copy remaining elements to the start of the item array.
                            int leftToCopy = c.Count - elementsAhead;
                            Array.Copy(temp, elementsAhead, _items, 0, leftToCopy);
                            _next = leftToCopy;
                        }
                    }
                    else // Copy into the space between _queueStart and _next, EnsureCapacity() will make sure the space is big enough.
                    {
                        c.CopyTo(_items, _next);
                        _next += c.Count;
                    }

                    _count += c.Count;
                    _isEmpty = _count == 0;
                }
            }
            else
            {
                // Since the collection is not an ICollection, we're forced to enumerate over items, one at a time.
                using (IEnumerator<T> e = collection.GetEnumerator())
                {
                    while (e.MoveNext())
                        Insert(e.Current);
                }
            }
        }

        bool IProducerConsumerCollection<T>.TryAdd(T item)
        {
            Enqueue(item);
            return true;
        }

        bool IProducerConsumerCollection<T>.TryTake(out T item)
        {
            return TryDequeue(out item);
        }

        public override string ToString() => $"Count: {_count}";

        /// <summary>
        /// Gets the number of items in the current <see cref="ThreadedQueue{T}"/>.
        /// </summary>
        public int Count => _count;

        object ICollection.SyncRoot => _locker;

        public bool IsSynchronized => true;
    }
}
