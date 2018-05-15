using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Molten.Collections
{
    /// <summary>A thread-safe list implementation. Basically wraps thread-safety around the vanilla list.</summary>
    /// <typeparam name="T">The type of object to be stored in the list.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    public partial class ThreadedList<T> : IList<T>, IProducerConsumerCollection<T>, IReadOnlyList<T>
    {
        static readonly T[] _emptyArray = new T[0];

        public T[] _items;
        int _count;
        int _capacity;
        int _version;

        object _locker;
        int _blockingVal;

        public ThreadedList()
        {
            _items = _emptyArray;
            _capacity = 0;
            _locker = new object();
            _blockingVal = 0;
        }

        public ThreadedList(int initialCapacity = 1)
        {
            if (initialCapacity < 0)
                throw new ArgumentOutOfRangeException("Cannot have a capacity less than 0.");

            _items = new T[initialCapacity];
            _capacity = _items.Length; 
            _locker = new object();
        }

        public ThreadedList(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            _items = _emptyArray;
            _capacity = 0;
            _locker = new object();
            _blockingVal = 0;
            AddRange(collection);
        }


        public void Add(T item)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    AddElement(item);
                    _version++;
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        public void AddRange(IEnumerable<T> collection)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    // Determine if enumerable is a colllection.
                    ICollection<T> c = collection as ICollection<T>;
                    if (c != null)
                    {
                        EnsureCapacityInternal(_count + c.Count);
                        c.CopyTo(_items, _count);
                        _count += c.Count;
                    }
                    else {
                        // Since the collection is not an ICollection, we're forced to enumerate over items, one at a time.
                        using (IEnumerator<T> e = collection.GetEnumerator())
                        {
                            while (e.MoveNext())
                                AddElement(e.Current);
                        }
                    }

                    // Release lock
                    _version++;
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        private void AddElement(T item) {
            EnsureCapacityInternal(_count + 1);
            _items[_count++] = item;
        }

        /// <summary>Ensures the list has at least the minimum specified capacity.</summary>
        /// <param name="min">The minimum capacity to ensure.</param>
        public void EnsureCapacity(int min) {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    EnsureCapacityInternal(min);
                    _version++;
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        /// <summary>Internal method for ensuring capacity.</summary>
        /// <param name="min"></param>
        private void EnsureCapacityInternal(int min)
        {
            if (min >= _items.Length)
            {
                int newCap = _capacity == 0 ? 1 : _capacity * 2;
                if (newCap < min) newCap = min;
                SetCapacity(newCap);
            }
        }

        /// <summary>Sets the internal capacity of the list.</summary>
        /// <param name="value">The total capacity required.</param>
        private void SetCapacity(int value)
        {
            if (value != _items.Length)
            {
                if (value < _count)
                    ThrowReleaseLock<IndexOutOfRangeException>("Capacity must be greater or equal to the number of stored items.");

                if (value > 0)
                {
                    T[] newItems = new T[value];
                    if (_count > 0)
                        Array.Copy(_items, 0, newItems, 0, _count);

                    _items = newItems;
                }
                else {
                    _items = _emptyArray;
                }

                _capacity = value;
            }
        }

        public void Clear()
        {
            SpinWait spin = new SpinWait();

            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    _count = 0;
                    _version++;
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        /// <summary>Copies the contents of the list to the provided array.</summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="index">The index within array to copy to.</param>
        public void CopyTo(Array array, int index)
        {
            CopyToInternal(array, index);
        }

        /// <summary>Copies the contents of the list to the provided array.</summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The index within array to copy to.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyToInternal(array, arrayIndex);
        }

        private void CopyToInternal(Array array, int index)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    int targetAvailable = array.Length - index;
                    if (targetAvailable < _count)
                    {
                        ThrowReleaseLock<IndexOutOfRangeException>("Target array does not have enough free space.");
                    }
                    else
                    {
                        Array.Copy(_items, 0, array, index, _count);

                        _version++;
                        Interlocked.Exchange(ref _blockingVal, 0);
                    }
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
                    bool found = false;
                    if ((Object)item == null)
                    {
                        for (int i = 0; i < _count; i++)
                        {
                            if ((Object)_items[i] == null)
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                    else {
                        EqualityComparer<T> c = EqualityComparer<T>.Default;
                        for (int i = 0; i < _count; i++)
                        {
                            if (c.Equals(_items[i], item))
                            {
                                found = true;
                                break;
                            }
                        }
                    }

                    // Release interlock and return result.
                    Interlocked.Exchange(ref _blockingVal, 0);
                    _version++;
                    return found;
                }
                spin.SpinOnce();
            }
        }


        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public int IndexOf(T item)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    int index = Array.IndexOf(_items, item);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return index;
                }
                spin.SpinOnce();
            }
        }

        public void Insert(int index, T item)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    InsertElement(item, index);
                    _version++;
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        public void InsertRange(IEnumerable<T> collection, int index)
        {
            SpinWait spin = new SpinWait();

            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    if (index == _count)
                    {
                        AddRange(collection);
                    }
                    else {
                        if (index > _count)
                        {
                            ThrowReleaseLock<IndexOutOfRangeException>("Cannot insert beyond the number of items in the list.");
                        }
                        else {
                            // Determine if enumerable is a colllection.
                            ICollection<T> c = collection as ICollection<T>;
                            if (c != null)
                            {
                                EnsureCapacityInternal(_count + c.Count);
                                Array.Copy(_items, index, _items, index + c.Count, _count - index);
                                c.CopyTo(_items, index);
                                _count += c.Count;
                            }
                            else {
                                // Since the collection does not implement ICollection, we're forced to enumerate over items, one at a time.
                                using (IEnumerator<T> e = collection.GetEnumerator())
                                {
                                    int startIndex = index;
                                    while (e.MoveNext())
                                    {
                                        InsertElement(e.Current, startIndex);
                                        startIndex++;
                                    }
                                }
                            }

                            _version++;
                        }
                    }

                    // Release lock
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        private void InsertElement(T item, int index)
        {
            if (index == _count)
            {
                AddElement(item);
            }
            else {
                if (index > _count)
                    ThrowReleaseLock<IndexOutOfRangeException>("Cannot insert beyond the number of items in the list.");
                else {
                    EnsureCapacityInternal(_count + 1);
                    Array.Copy(_items, index, _items, index + 1, _count - index); // Move items in front up by one index.
                    _items[index] = item;
                    _count++;
                }
            }
        }

        /// <summary>Removes an item from the list.</summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    int index = Array.IndexOf<T>(_items, item);
                    bool found = (index > -1);
                    if (found)
                    {
                        RemoveElement(index);
                        _version++;
                    }

                    Interlocked.Exchange(ref _blockingVal, 0);
                    return found;
                }
                spin.SpinOnce();
            }
        }

        /// <summary>Removes an item from the list at the specified index.</summary>
        /// <param name="index">The index.</param>
        public void RemoveAt(int index)
        {
            if (index < 0)
                throw new IndexOutOfRangeException("Index cannot be less than 0.");

            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    if (index >= _count)
                        ThrowReleaseLock<IndexOutOfRangeException>("Index must be less than the item count.");

                    RemoveElement(index);
                    _version++;
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        public void RemoveRange(int index, int count)
        {
            if (index < 0)
                throw new IndexOutOfRangeException("Index cannot be less than 0.");

            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    int lastElement = index + count;

                    if (index >= _count)
                        ThrowReleaseLock<IndexOutOfRangeException>("Index must be less than the item count.");
                    else if (lastElement > _count)
                        ThrowReleaseLock<IndexOutOfRangeException>("Index plus count cannot exceed the number of items stored in the list.");


                    Array.Copy(_items, lastElement, _items, index, _count - lastElement);
                    _count -= count;
                    Array.Clear(_items, _count, count); // Clear old spaces of re-positioned elements.

                    _version++;
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        public int RemoveAll(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException("Match cannot be null.");

            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    int result = 0;
                    int freeIndex = 0;   // the first free slot in items array

                    // Find the first item which needs to be removed.
                    while (freeIndex < _count && !match(_items[freeIndex])) freeIndex++;
                    if (freeIndex >= _count)
                    {
                        result = 0;
                    }
                    else {
                        int current = freeIndex + 1;
                        while (current < _count)
                        {
                            // Find the first item which needs to be kept.
                            while (current < _count && match(_items[current])) current++;

                            if (current < _count)
                            {
                                // copy item to the free slot.
                                _items[freeIndex++] = _items[current++];
                            }
                        }

                        Array.Clear(_items, freeIndex, _count - freeIndex);
                        result = _count - freeIndex;
                        _count = freeIndex;
                        _version++;
                    }

                    // Release interlock.
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return result;
                }
                spin.SpinOnce();
            }
        }

        private void RemoveElement(int index)
        {
            _count--;

            // If index was not the last element, update array structure to close gap.
            if (index != _count)
            {
                Array.Copy(_items, index + 1, _items, index, _count - index); // Move items ahead of the index, back one element.
                _items[_count] = default(T); // Clear last element, since it moved back by one index.
            }
            else
            {
                _items[index] = default(T);
            }
        }

        public void Reverse()
        {
            Reverse(0, _count);
        }

        /// <summary>Reverses the elements in a range of this list. Following a call to this method, 
        /// an element in the range given by index and count which was previously located 
        /// at index i will now be located at index index + (index + count - i - 1). 
        /// This method uses the Array.Reverse method to reverse the elements.</summary>
        /// <param name="index">The index to start reversing elements.</param>
        /// <param name="count">The number of elements to reverse</param>
        public void Reverse(int index, int count)
        {
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than 0");
            else if (index < 0)
                throw new IndexOutOfRangeException("Index cannot be less than 0.");

            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    if (_count - index < count)
                        ThrowReleaseLock<ArgumentException>("Index and count will go out of bounds. Invalid.");

                    Array.Reverse(_items, index, count);
                    _version++;
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        /// Sorts the elements in this list.  Uses the default comparer and 
        /// Array.Sort.
        public void Sort()
        {
            Sort(0, Count, null);
        }

        /// Sorts the elements in this list.  Uses Array.Sort with the
        /// provided comparer.
        public void Sort(IComparer<T> comparer)
        {
            Sort(0, Count, comparer);
        }

        /// <summary>Sorts the elements in a section of this list. The sort compares the 
        /// elements to each other using the given IComparer interface. If
        /// comparer is null, the elements are compared to each other using 
        /// the IComparable interface, which in that case must be implemented by all 
        /// elements of the list.
        /// This method uses the Array.Sort method to sort the elements. </summary>
        /// <param name="index">The index at which to start sorting.</param>
        /// <param name="count">The number of items to sort.</param>
        /// <param name="comparer">A comparer used for performing sort operation and validation.</param>
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            if (index < 0)
                throw new IndexOutOfRangeException("Index cannot be less than 0.");
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than 0.");

            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    if (_count - index < count)
                        ThrowReleaseLock<ArgumentException>("Index plus count cannot exceed the number of items in the list.");

                    Array.Sort<T>(_items, index, count, comparer);
                    _version++;
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        /// <summary>Sets the capacity of this list to the size of the list. This method can 
        /// be used to minimize a list's memory overhead once it is known that no
        /// new elements will be added to the list. To completely clear a list and 
        /// release all memory referenced by the list.</summary>
        public void TrimExcess()
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    SetCapacity(_count);
                    _version++;
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        /// <summary>Copies the contents of the list to an array and returns it.</summary>
        /// <returns>An array containing the list contents.</returns>
        public T[] ToArray()
        {
            SpinWait spin = new SpinWait();

            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    T[] result = new T[_count];
                    if(_count > 0)
                        Array.Copy(_items, 0, result, 0, _count);

                    Interlocked.Exchange(ref _blockingVal, 0);
                    return result;
                }
                spin.SpinOnce();
            }
        }

        /// <summary>Tries to add an item to the list. This will always succeed due to the lock-free nature of <see cref="ThreadedList{T}"/></summary>
        /// <param name="item">The item to be added.</param>
        /// <returns></returns>
        public bool TryAdd(T item)
        {
            Add(item);
            return true;
        }

        /// <summary>Remove's an item from the end of the list and returns it. This replicates stack functionality (FILO).</summary>
        /// <param name="item">An output for the returned item.</param>
        /// <returns>True if an item was taken.</returns>
        public bool TryTake(out T item)
        {
            SpinWait spin = new SpinWait();

            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    bool hasItem = (_count > 0);
                    if (hasItem)
                    {
                        item = _items[--_count];
                    }
                    else {
                        item = default(T);
                    }

                    _version++;
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return hasItem;
                }
                spin.SpinOnce();
            }
        }

        /// <summary>Runs a for loop inside an interlock on the list. This allows iteration with only interlocking once. This can hurt performance if
        /// the loop takes too long to execute while other threads are waiting to access the list. Return true from the callback to break out of the for loop.</summary>
        /// <param name="start">The start index.</param>
        /// <param name="increment">The increment.</param>
        /// <param name="callback">The callback to run on each iteration. The callback should return true to break out of the loop.</param>
        public void ForInterlock(int start, int increment, Func<int, T, bool> callback)
        {
            SpinWait spin = new SpinWait();

            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    for (int i = start; i < _count; i += increment)
                    {
                        if (callback(i, _items[i]))
                            break;
                    }
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        /// <summary>Runs a for loop inside an interlock on the list. This allows iteration with only interlocking once. This can hurt performance if
        /// the loop takes too long to execute while other threads are waiting to access the list. Return true from the callback to break out of the for loop.</summary>
        /// <param name="start">The start index.</param>
        /// <param name="increment">The increment.</param>
        /// <param name="end">The element to iterate up to.</param>
        /// <param name="callback">The callback to run on each iteration. The callback should return true to break out of the loop.</param>
        public void ForInterlock(int start, int increment, int end, Func<int, T, bool> callback)
        {
            SpinWait spin = new SpinWait();

            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    // Figure out which is the smallest condition value. 
                    int last = Math.Min(_count, end);

                    for (int i = start; i < last; i += increment)
                    {
                        if (callback(i, _items[i]))
                            break;
                    }
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        /// <summary>Runs a for loop inside an interlock on the list, backwards. This allows iteration with only interlocking once. This can hurt performance if
        /// the loop takes too long to execute while other threads are waiting to access the list. Return true from the callback to break out of the for loop.</summary>
        /// <param name="start">The start index.</param>
        /// <param name="decrement">The decremental value.</param>
        /// <param name="callback">The callback to run on each iteration. The callback should return true to break out of the loop.</param>
        public void ForInterlockReverse(int decrement, Func<int, T, bool> callback)
        {
            SpinWait spin = new SpinWait();

            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    int start = _count - 1;
                    for (int i = start; i >= 0; i -= decrement)
                    {
                        if (callback(i, _items[i]))
                            break;
                    }
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        /// <summary>Runs a for loop inside an interlock on the list, backwards. This allows iteration with only interlocking once. This can hurt performance if
        /// the loop takes too long to execute while other threads are waiting to access the list. Return true from the callback to break out of the for loop.</summary>
        /// <param name="start">The start index.</param>
        /// <param name="decrement">The decremental value.</param>
        /// <param name="callback">The callback to run on each iteration. The callback should return true to break out of the loop.</param>
        public void ForInterlockReverse(int start, int decrement, Func<int, T, bool> callback)
        {
            SpinWait spin = new SpinWait();

            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    for (int i = start; i >= 0; i -= decrement)
                    {
                        if (callback(i, _items[i]))
                            break;
                    }
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        /// <summary>Gets or sets a value at the given index.</summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                SpinWait spin = new SpinWait();
                while (true)
                {
                    if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                    {
                        if (index >= _count)
                            ThrowReleaseLock<IndexOutOfRangeException>("Index must be less than item count.");
                        else if (index < 0)
                            ThrowReleaseLock<IndexOutOfRangeException>("Index cannot be less than 0.");

                        T result = _items[index];
                        Interlocked.Exchange(ref _blockingVal, 0);
                        return result;
                    }
                    spin.SpinOnce();
                }
            }

            set
            {
                SpinWait spin = new SpinWait();
                while (true)
                {
                    if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                    {
                        if (index >= _count)
                            ThrowReleaseLock<IndexOutOfRangeException>("Index must be less than item count.");
                        else if (index < 0)
                            ThrowReleaseLock<IndexOutOfRangeException>("Index cannot be less than 0.");

                        _items[index] = value;
                        _version++;
                        Interlocked.Exchange(ref _blockingVal, 0);
                        return;
                    }
                    spin.SpinOnce();
                }
            }
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

        /// <summary>Gets the number of items in the list.</summary>
        public int Count
        {
            get { return _count; }
        }

        /// <summary>Gets whether or not the list is read-only.</summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>Gets whether the List synchronized (thread-safe).</summary>
        public bool IsSynchronized
        {
            get { return true; }
        }

        public object SyncRoot
        {
            get { return _locker; }
        }

        public int Capacity
        {
            get { return _capacity; }
            set
            {
                SetCapacity(value);
            }
        }
    }
}