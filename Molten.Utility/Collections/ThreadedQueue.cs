using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Molten.Collections
{
    public partial class ThreadedQueue<T> : IEnumerable<T>, ICollection, IEnumerable, IProducerConsumerCollection<T>
    {
        T[] _items;

        bool _isEmpty;
        int _count;
        int _next;
        int _queueStart; // The start of the queue, where items are dequeued.

        object _locker;
        int _blockingVal;

        public ThreadedQueue(int initialCapacity = 20)
        {
            _items = new T[initialCapacity];
            _locker = new object();
            _isEmpty = true;
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>Clears all items from the queue. This will not release the memory consumed by the internal array. To do so, the queue must be disposed.</summary>
        public void Clear()
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    if (_next > _queueStart)
                    {
                        for (int i = _queueStart; i < _next; i++)
                            _items[i] = default(T);
                    }
                    else {
                        for (int i = _queueStart; i < _items.Length; i++)
                            _items[i] = default(T);

                        for (int i = 0; i < _next; i++)
                            _items[i] = default(T);
                    }
                    _next = 0;
                    _count = 0;
                    _queueStart = 0;
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        public void CopyTo(Array array, int index)
        {
            T[] destination = array as T[];

            if (destination == null)
                throw new InvalidOperationException("The target array is not of the correct type");

            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    CopyToArray(destination, index);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    break;
                }
                spin.SpinOnce();
            }
        }

        public void CopyTo(T[] array, int index)
        {
            SpinWait spin = new SpinWait();

            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    CopyToArray(array, index);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    break;
                }
                spin.SpinOnce();
            }
        }

        public T[] ToArray()
        {
            SpinWait spin = new SpinWait();
            T[] result = null;

            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    result = new T[_count];
                    CopyToArray(result, 0);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    break;
                }
                spin.SpinOnce();
            }

            return result;
        }

        /// <summary>Copies the queue into the provided array, cutting out any whitespace in the process.</summary>
        /// <param name="target">The destination array.</param>
        /// <param name="destIndex">The index in the target array to start copying to.</param>
        private void CopyToArray(T[] target, int destIndex)
        {
            if (target.Length - destIndex < _count)
                ThrowReleaseLock<IndexOutOfRangeException>("The target array is not large enough to accept the copy operation");

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
        }

        public void Enqueue(T item)
        {
            SpinWait spin = new SpinWait();

            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    Insert(item);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        public void EnqueueRange(IEnumerable<T> range)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    InsertRange(range);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        public bool TryDequeue(out T item)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    bool hasItems = !_isEmpty;

                    if (hasItems)
                        item = GetItem();
                    else
                        item = default(T);

                    // Release lock and return
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return hasItems;
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
                    bool found = false;
                    if ((Object)item == null)
                    {
                        // Check if iteration needs to wrap around.
                        if (_queueStart < _next)
                        {
                            for (int i = _queueStart; i < _next; i++)
                            {
                                if ((Object)_items[i] == null)
                                {
                                    found = true;
                                    goto exitSearch;
                                }
                            }
                        }
                        else {
                            // from start of queue to end of array.
                            for (int i = _queueStart; i < _items.Length; i++)
                            {
                                if ((Object)_items[i] == null)
                                {
                                    found = true;
                                    goto exitSearch;
                                }
                            }

                            // From 0 to _next
                            for (int i = 0; i < _next; i++)
                            {
                                if ((Object)_items[i] == null)
                                {
                                    found = true;
                                    goto exitSearch;
                                }
                            }
                        }
                    }
                    else {
                        EqualityComparer<T> c = EqualityComparer<T>.Default;
                        if (_queueStart < _next)
                        {
                            for (int i = 0; i < _count; i++)
                            {
                                if (c.Equals(_items[i], item))
                                {
                                    found = true;
                                    goto exitSearch;
                                }
                            }
                        }
                        else {
                            // from start of queue to end of array.
                            for (int i = _queueStart; i < _items.Length; i++)
                            {
                                if (c.Equals(_items[i], item))
                                {
                                    found = true;
                                    goto exitSearch;
                                }
                            }

                            // From 0 to _next
                            for (int i = 0; i < _next; i++)
                            {
                                if (c.Equals(_items[i], item))
                                {
                                    found = true;
                                    goto exitSearch;
                                }
                            }
                        }
                    }

                    exitSearch:
                    // Release lock and return
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return found;
                }
                spin.SpinOnce();
            }
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
                if(_next > 0)
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

        /// <summary>Throws an exception of the specified type and resets the interlocking value.</summary>
        /// <typeparam name="E">The type of exception to throw.</typeparam>
        /// <param name="message">The exception message.</param>
        private void ThrowReleaseLock<E>(string message) where E : Exception
        {
            E ex = Activator.CreateInstance(typeof(E), message) as E;
            Interlocked.Exchange(ref _blockingVal, 0);
            throw ex;
        }

        public override string ToString()
        {
            return $"Count: {_count}";
        }

        public int Count
        {
            get { return _count; }
        }

        public object SyncRoot
        {
            get { return _locker; }
        }

        public bool IsSynchronized { get { return true; } }
    }
}
