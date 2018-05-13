using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Molten.Collections
{
    /// <summary>A thread-safe, lock-free wrap of a standard <see cref="Dictionary{K, V}"/></summary>
    /// <typeparam name="K">The key type.</typeparam>
    /// <typeparam name="V">The value type.</typeparam>
    public class ThreadedDictionary<K, V> : IDictionary<K, V>, ICollection<KeyValuePair<K, V>>, IDictionary,
        ICollection, IReadOnlyDictionary<K, V>, IReadOnlyCollection<KeyValuePair<K, V>>, IEnumerable<KeyValuePair<K, V>>, IEnumerable
    {
        Dictionary<K, V> _dictionary;
        int _blockingVal;
        Type _keyType;
        Type _valueType;

        public ThreadedDictionary()
        {
            _dictionary = new Dictionary<K, V>();
            _keyType = typeof(K);
            _valueType = typeof(V);
        }

        public ThreadedDictionary(IDictionary<K, V> dictionary, IEqualityComparer<K> comparer)
        {
            _dictionary = new Dictionary<K, V>(dictionary, comparer);
        }

        public ThreadedDictionary(int capacity, IEqualityComparer<K> comparer)
        {
            _dictionary = new Dictionary<K, V>(capacity, comparer);
        }

        public ThreadedDictionary(IDictionary<K, V> dictionary)
        {
            _dictionary = new Dictionary<K, V>(dictionary);
        }

        private void ThrowReleaseLock<T>(string msg) where T : Exception
        {
            T e = Activator.CreateInstance(typeof(T), msg) as T;
            ThrowReleaseLock(e);
        }

        private void ThrowReleaseLock(Exception e)
        {
            Interlocked.Exchange(ref _blockingVal, 0);
            throw e;
        }

        object IDictionary.this[object key]
        {
            get
            {
                SpinWait spin = new SpinWait();
                while (true)
                {
                    if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                    {
                        V result = default(V);
                        if (key.GetType() != _keyType)
                            ThrowReleaseLock<ArgumentException>("The key is not a compatible type.");

                        K castedKey = (K)key;
                        _dictionary.TryGetValue(castedKey, out result);

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
                        V result = default(V);
                        if (key.GetType() != _keyType)
                            ThrowReleaseLock<ArgumentException>("The key is not a compatible type.");

                        K castedKey = (K)key;
                        _dictionary.TryGetValue(castedKey, out result);

                        Interlocked.Exchange(ref _blockingVal, 0);
                        return;
                    }
                    spin.SpinOnce();
                }
            }
        }

        public V this[K key]
        {
            get
            {
                SpinWait spin = new SpinWait();
                while (true)
                {
                    if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                    {
                        V result = default(V);
                        _dictionary.TryGetValue(key, out result);
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
                        try
                        {
                            _dictionary[key] = value;
                        }
                        catch { }

                        Interlocked.Exchange(ref _blockingVal, 0);
                        return;
                    }
                    spin.SpinOnce();
                }
            }
        }

        /// <summary>Gets the number of entries stored in the dictionary.</summary>
        public int Count
        {
            get { return _dictionary.Count; }
        }

        /// <summary>Gets whether or not the dictionary is fixed size. Always false.</summary>
        public bool IsFixedSize
        {
            get { return false; }
        }

        /// <summary>Gets whether or not the dictionary is read-only.</summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets whether or not the dictionary is synchronized. Always true.
        /// </summary>
        public bool IsSynchronized
        {
            get { return true; }
        }

        /// <summary>Gets the dictionary's keys as a collection. Thread-safe.</summary>
        public ICollection<K> Keys
        {
            get
            {
                SpinWait spin = new SpinWait();
                while (true)
                {
                    if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                    {
                        ICollection<K> col = _dictionary.Keys;
                        Interlocked.Exchange(ref _blockingVal, 0);
                        return col;
                    }
                    spin.SpinOnce();
                }
            }
        }

        public object SyncRoot
        {
            get { throw new NotSupportedException("ThreadedDictionary does not support a SyncRoot object. The collection is thread-safe by design."); }
        }

        /// <summary>Gets the dictionary's values as a collection. Thread-safe.</summary>
        public ICollection<V> Values
        {
            get
            {
                SpinWait spin = new SpinWait();
                while (true)
                {
                    if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                    {
                        ICollection<V> col = _dictionary.Values;
                        Interlocked.Exchange(ref _blockingVal, 0);
                        return col;
                    }
                    spin.SpinOnce();
                }
            }
        }
        /// <summary>
        /// Gets a read-only collection containing all of the values stored within the dictionary.
        /// </summary>
        IEnumerable<K> IReadOnlyDictionary<K, V>.Keys
        {
            get { return this.Keys; }
        }

        /// <summary>
        /// Gets a collection containing all of the keys stored within the dictionary.
        /// </summary>
        ICollection IDictionary.Keys
        {
            get { return this.Keys as ICollection; }
        }

        /// <summary>
        /// Gets a read-only collection containing all of the values stored within the dictionary.
        /// </summary>
        IEnumerable<V> IReadOnlyDictionary<K, V>.Values
        {
            get { return this.Values; }
        }

        /// <summary>
        /// Gets a collection containing all of the values stored within the dictionary.
        /// </summary>
        ICollection IDictionary.Values
        {
            get { return this.Values as ICollection; }
        }

        /// <summary>Attempts to add the item to the dictionary. Does nothing if the item has already been added.</summary>
        /// <param name="pair">The key-value pair to be added.</param>
        public void Add(KeyValuePair<K, V> pair)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    if (pair.Key == null)
                        ThrowReleaseLock<ArgumentNullException>("Key cannot be null");

                    if (!_dictionary.ContainsKey(pair.Key))
                        _dictionary.Add(pair.Key, pair.Value);

                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        void IDictionary.Add(object key, object value)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    if (key == null)
                        ThrowReleaseLock<ArgumentNullException>("Key cannot be null");

                    if (!_keyType.IsAssignableFrom(key.GetType()))
                        ThrowReleaseLock<ArgumentException>("Key is not of a compatible type.");

                    if (!_valueType.IsAssignableFrom(value.GetType()))
                        ThrowReleaseLock<ArgumentException>("Value is not a compatible type.");

                    if (!_dictionary.ContainsKey((K)key))
                        _dictionary.Add((K)key, (V)value);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        public void Add(K key, V value)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    if (key == null)
                        ThrowReleaseLock<ArgumentNullException>("Key cannot be null");

                    if (!_dictionary.ContainsKey(key))
                        _dictionary.Add(key, value);

                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        public bool TryAdd(K key, V value)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    if (key == null)
                        ThrowReleaseLock<ArgumentNullException>("Key cannot be null");

                    bool result = !_dictionary.ContainsKey(key);
                    if (result)
                        _dictionary.Add(key, value);
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
                    _dictionary.Clear();
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        /// <summary>Checks if a key is contained in the dictionary.</summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(object key)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    if (key.GetType() != _keyType)
                        ThrowReleaseLock<ArgumentException>("Key is not of a compatible type.");

                    K castKey = (K)key;
                    bool result = _dictionary.ContainsKey(castKey);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return result;
                }
                spin.SpinOnce();
            }
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    bool result = _dictionary.Contains(item);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return result;
                }
                spin.SpinOnce();
            }
        }

        public bool ContainsKey(K key)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    bool result = _dictionary.ContainsKey(key);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return result;
                }
                spin.SpinOnce();
            }
        }

        /// <summary>Copies the contents of the dictionary to an array.</summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The index to start placing the copied data at.</param>
        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    KeyValuePair<K, V>[] data = _dictionary.ToArray();
                    Array.Copy(data, 0, array, arrayIndex, data.Length);
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
                    KeyValuePair<K, V>[] data = _dictionary.ToArray();
                    Array.Copy(data, 0, array, index, data.Length);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    IEnumerator<KeyValuePair<K, V>> e = _dictionary.GetEnumerator();
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return e;
                }
                spin.SpinOnce();
            }
        }

        public void Remove(object key)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    if (key.GetType() != _keyType)
                        ThrowReleaseLock<ArgumentException>("Key is not of a compatible type.");

                    K castKey = (K)key;
                    _dictionary.Remove(castKey);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            throw new NotSupportedException("ThreadedDictionary does not support adding key-value pairs.");
        }

        public bool Remove(K key)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    bool result = _dictionary.Remove(key);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return result;
                }
                spin.SpinOnce();
            }
        }

        /// <summary>Attempts to retrieve an item from the dictionary, then remove it.</summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryRemoveValue(K key, out V value)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    bool result = _dictionary.TryGetValue(key, out value);
                    if (result)
                        _dictionary.Remove(key);

                    Interlocked.Exchange(ref _blockingVal, 0);
                    return result;
                }
                spin.SpinOnce();
            }
        }

        public bool TryGetValue(K key, out V value)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    bool result = _dictionary.TryGetValue(key, out value);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return result;
                }
                spin.SpinOnce();
            }
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    IDictionaryEnumerator e = _dictionary.GetEnumerator();
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return e;
                }
                spin.SpinOnce();
            }
        }

        IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator()
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    IEnumerator<KeyValuePair<K, V>> e = _dictionary.GetEnumerator();
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return e;
                }
                spin.SpinOnce();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    IEnumerator e = _dictionary.GetEnumerator();
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return e;
                }
                spin.SpinOnce();
            }
        }

        public void CopyTo(V[] array, int index)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    ICollection<V> data = _dictionary.Values;
                    data.CopyTo(array, index);
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }

        /// <summary>Copies all dictionary keys to an array and returns it.</summary>
        /// <returns></returns>
        public K[] ToKeyArray()
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    K[] data = _dictionary.Keys.ToArray();
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return data;
                }
                spin.SpinOnce();
            }
        }

        /// <summary>Copies all dictionary values to an array and returns it.</summary>
        /// <returns></returns>
        public V[] ToValueArray()
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                {
                    V[] data = _dictionary.Values.ToArray();
                    Interlocked.Exchange(ref _blockingVal, 0);
                    return data;
                }
                spin.SpinOnce();
            }
        }

        /// <summary>Waits until the dictionary interlock can be taken. Returns 1 when successful.</summary>
        /// <returns></returns>
        public int WaitInterlock()
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                if (0 == Interlocked.Exchange(ref _blockingVal, 1))
                    return 1;
                else
                    spin.SpinOnce();
            }
        }

        /// <summary>Releases the interlock value if the provided value is 1.</summary>
        /// <param name="val">The interlocking value.</param>
        public void ReleaseInterlock(int val)
        {
            if (val != 1)
                return;

            Interlocked.Exchange(ref _blockingVal, val);
        }
    }
}
