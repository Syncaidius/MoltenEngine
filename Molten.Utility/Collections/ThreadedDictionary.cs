using System.Collections;

namespace Molten.Collections
{
    /// <summary>A thread-safe, lock-free wrap of a standard <see cref="Dictionary{K, V}"/></summary>
    /// <typeparam name="K">The key type.</typeparam>
    /// <typeparam name="V">The value type.</typeparam>
    public class ThreadedDictionary<K, V> : IDictionary<K, V>, ICollection<KeyValuePair<K, V>>, IDictionary,
        ICollection, IReadOnlyDictionary<K, V>, IReadOnlyCollection<KeyValuePair<K, V>>, IEnumerable<KeyValuePair<K, V>>, IEnumerable
    {
        Dictionary<K, V> _dictionary;
        Interlocker _interlocker;
        Type _keyType;
        Type _valueType;

        /// <summary>
        /// Creates a new instance of <see cref="ThreadedDictionary{K, V}"/>.
        /// </summary>
        public ThreadedDictionary() : this(new Dictionary<K, V>()) { }

        public ThreadedDictionary(IDictionary<K, V> dictionary, IEqualityComparer<K> comparer)
        {
            _dictionary = new Dictionary<K, V>(dictionary, comparer);
            _keyType = typeof(K);
            _valueType = typeof(V);
            _interlocker = new Interlocker();
        }

        public ThreadedDictionary(int capacity, IEqualityComparer<K> comparer)
        {
            _dictionary = new Dictionary<K, V>(capacity, comparer);
            _keyType = typeof(K);
            _valueType = typeof(V);
            _interlocker = new Interlocker();
        }

        /// <summary>
        /// Creates a new instance of <see cref="ThreadedDictionary{K, V}"/>, then populates it with the contents of the provided <see cref="IDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="dictionary"></param>
        public ThreadedDictionary(IDictionary<K, V> dictionary)
        {
            _dictionary = new Dictionary<K, V>(dictionary);
            _keyType = typeof(K);
            _valueType = typeof(V);
            _interlocker = new Interlocker();
        }

        object IDictionary.this[object key]
        {
            get
            {
                if (!_keyType.IsAssignableFrom(key.GetType()))
                    throw new ArgumentException("The key is not a compatible type.");

                V result = default(V);
                _interlocker.Lock(() => _dictionary.TryGetValue((K)key, out result));
                return result;
            }

            set
            {
                if (!_keyType.IsAssignableFrom(key.GetType()))
                    throw new ArgumentException("The key is not a compatible type.");

                if (!_valueType.IsAssignableFrom(value.GetType()))
                    throw new ArgumentException("The value is not a compatible type.");

                _interlocker.Lock(() => _dictionary[(K)key] = (V)value);
            }
        }

        /// <summary>
        /// Gets or sets a value with the specified key in the dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public V this[K key]
        {
            get
            {
                V result = default(V);
                _interlocker.Lock(() => _dictionary.TryGetValue(key, out result));
                return result;
            }

            set => _interlocker.Lock(() => _dictionary[key] = value);
        }

        /// <summary>Gets the number of entries stored in the dictionary.</summary>
        public int Count => _dictionary.Count;

        /// <summary>Gets whether or not the dictionary is fixed size. Always false.</summary>
        public bool IsFixedSize => false;

        /// <summary>Gets whether or not the dictionary is read-only.</summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets whether or not the dictionary is synchronized. Always true.
        /// </summary>
        public bool IsSynchronized => true;

        /// <summary>Gets the dictionary's keys as a collection. Thread-safe.</summary>
        public ICollection<K> Keys
        {
            get
            {
                ICollection<K> col = null;
                _interlocker.Lock(() => col = _dictionary.Keys);
                return col;
            }
        }

        object ICollection.SyncRoot
        {
            get { throw new NotSupportedException("ThreadedDictionary does not support a SyncRoot object. The collection is thread-safe by design."); }
        }

        /// <summary>Gets the dictionary's values as a collection. Thread-safe.</summary>
        public ICollection<V> Values
        {
            get
            {
                ICollection<V> col = null;
                _interlocker.Lock(() => col = _dictionary.Values);
                return col;
            }
        }
        /// <summary>
        /// Gets a read-only collection containing all of the values stored within the dictionary.
        /// </summary>
        IEnumerable<K> IReadOnlyDictionary<K, V>.Keys => this.Keys;

        /// <summary>
        /// Gets a collection containing all of the keys stored within the dictionary.
        /// </summary>
        ICollection IDictionary.Keys => this.Keys as ICollection;

        /// <summary>
        /// Gets a read-only collection containing all of the values stored within the dictionary.
        /// </summary>
        IEnumerable<V> IReadOnlyDictionary<K, V>.Values => this.Values;

        /// <summary>
        /// Gets a collection containing all of the values stored within the dictionary.
        /// </summary>
        ICollection IDictionary.Values => this.Values as ICollection;

        /// <summary>Attempts to add the item to the dictionary. Does nothing if the item has already been added.</summary>
        /// <param name="pair">The key-value pair to be added.</param>
        public void Add(KeyValuePair<K, V> pair)
        {
            if (pair.Key == null)
                throw new ArgumentNullException("Key cannot be null");

            _interlocker.Lock(() =>
            {
                if (!_dictionary.ContainsKey(pair.Key))
                    _dictionary.Add(pair.Key, pair.Value);
                else
                    _interlocker.Throw<ArgumentException>("A matching key already exists in the dictionary.");
            });
        }

        void IDictionary.Add(object key, object value)
        {
            if (key == null)
                throw new ArgumentNullException("Key cannot be null");

            if (!_keyType.IsAssignableFrom(key.GetType()))
                throw new ArgumentNullException("Key is not of a compatible type.");

            if (!_valueType.IsAssignableFrom(value.GetType()))
                throw new ArgumentNullException("Value is not a compatible type.");

            _interlocker.Lock(() =>
            {
                if (!_dictionary.ContainsKey((K)key))
                    _dictionary.Add((K)key, (V)value);
                else
                    _interlocker.Throw<ArgumentException>("A matching key already exists in the dictionary.");
            });
        }

        public void Add(K key, V value)
        {
            if (key == null)
                throw new ArgumentNullException("Key cannot be null");

            _interlocker.Lock(() =>
            {
                if (!_dictionary.ContainsKey(key))
                    _dictionary.Add(key, value);
                else
                    _interlocker.Throw<ArgumentException>("A matching key already exists in the dictionary.");
            });
        }

        public bool TryAdd(K key, V value)
        {
            if (key == null)
                throw new ArgumentNullException("Key cannot be null");

            bool result = false;
            _interlocker.Lock(() =>
            {
                if (!_dictionary.ContainsKey(key))
                {
                    _dictionary.Add(key, value);
                    result = true;
                }
            });

            return result;
        }

        public void Clear()
        {
            _interlocker.Lock(() => _dictionary.Clear());
        }

        /// <summary>
        /// Returns true if the dictionary contains the specified key. Returns false if the pair is not found.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        bool IDictionary.Contains(object key)
        {
            if (!_keyType.IsAssignableFrom(key.GetType()))
                throw new ArgumentNullException("Key is not of a compatible type.");

            bool result = false;
            _interlocker.Lock(() => result = _dictionary.ContainsKey((K)key));
            return result;
        }

        /// <summary>
        /// Returns true if the dictionary contains the specified key-value pair. Returns false if the pair is not found.
        /// </summary>
        /// <param name="pair">The key-value pair.</param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<K, V> pair)
        {
            bool result = false;
            _interlocker.Lock(() => result = _dictionary.Contains(pair));
            return result;
        }

        /// <summary>
        /// Returns true if the dictionary contains the specified key. Returns false if the key is not found.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool ContainsKey(K key)
        {
            bool result = false;
            _interlocker.Lock(() => result = _dictionary.ContainsKey(key));
            return result;
        }

        /// <summary>Copies the contents of the dictionary to an array.</summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The index to start placing the copied data at.</param>
        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            _interlocker.Lock(() =>
            {
                KeyValuePair<K, V>[] data = _dictionary.ToArray();
                Array.Copy(data, 0, array, arrayIndex, data.Length);
            });
        }

        /// <summary>Copies the contents of the dictionary to an array.</summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The index to start placing the copied data at.</param>
        public void CopyTo(Array array, int arrayIndex)
        {
            _interlocker.Lock(() =>
            {
                KeyValuePair<K, V>[] data = _dictionary.ToArray();
                Array.Copy(data, 0, array, arrayIndex, data.Length);
            });
        }

        /// <summary>
        /// Gets an enumerator for the dictioanry.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            IEnumerator<KeyValuePair<K, V>> enumerator = null;
            _interlocker.Lock(() => enumerator = _dictionary.GetEnumerator());
            return enumerator;
        }

        /// <summary>
        /// Removes an object with the specified key from the dictionary.
        /// </summary>
        /// <param name="key">The key of the object to be removed.</param>
        void IDictionary.Remove(object key)
        {
            if (!_keyType.IsAssignableFrom(key.GetType()))
                throw new ArgumentNullException("Key is not of a compatible type.");

            _interlocker.Lock(() => _dictionary.Remove((K)key));
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            bool result = false;
            _interlocker.Lock(() => result = _dictionary.Remove(item.Key));
            return result;
        }

        public bool Remove(K key)
        {
            bool result = false;
            _interlocker.Lock(() => result = _dictionary.Remove(key));
            return result;
        }

        /// <summary>Attempts to retrieve an item from the dictionary, then remove it.</summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The output destination of the retrieved value. If no value is retrieved, <see cref="default(V)"/> will be returned.</param>
        /// <returns></returns>
        public bool TryRemoveValue(K key, out V value)
        {
            bool result = false;
            V temp = default(V);

            _interlocker.Lock(() =>
            {
                if (_dictionary.TryGetValue(key, out temp))
                    result = _dictionary.Remove(key);
            });
            value = temp;
            return result;
        }

        public bool TryGetValue(K key, out V value)
        {
            bool result = false;
            V temp = default(V);
            _interlocker.Lock(() => result = _dictionary.TryGetValue(key, out temp));
            value = temp;
            return result;
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            IDictionaryEnumerator enumerator = null;
            _interlocker.Lock(() => _dictionary.GetEnumerator());
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerator enumerator = null;
            _interlocker.Lock(() => _dictionary.GetEnumerator());
            return enumerator;
        }

        /// <summary>
        /// Copies the <see cref="KeyValuePair{TKey, TValue}"/> objects contained in the dictionary, to the specified array.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="index">The index in the destination array at which to begin copying.</param>
        public void CopyTo(V[] array, int index)
        {
            _interlocker.Lock(() =>
            {
                ICollection<V> data = _dictionary.Values;
                data.CopyTo(array, index);
            });
        }

        /// <summary>Copies all dictionary keys to an array and returns it.</summary>
        /// <returns></returns>
        public K[] ToKeyArray()
        {
            K[] result = null;
            _interlocker.Lock(() => result = _dictionary.Keys.ToArray());
            return result;
        }

        /// <summary>Copies all dictionary values to an array and returns it.</summary>
        /// <returns></returns>
        public V[] ToValueArray()
        {
            V[] result = null;
            _interlocker.Lock(() => result = _dictionary.Values.ToArray());
            return result;
        }
    }
}
