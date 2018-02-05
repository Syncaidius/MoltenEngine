using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Molten.Collections
{
    /// <summary>A dictionary-esque data structure for storing key and value pairs without all of the baggage and 
    /// performance overhead of the default dictionary.</summary>
    public class SpeedDictionary<K, T>
    {
        /// <summary>The array containing the keys of the matching data entries. The ID of a key is the same as its data entry.</summary>
        public K[] Keys;

        /// <summary>The array containing the contents of the table.</summary>
        public T[] Data;
        private int _itemCount;
        private int _actualCount;

        private int[] _freeIDs;
        private int _freeCount;

        ExpansionMode _expansionMode;
        int _expansionValue;

        int _iterationPos;

        /// <summary>Creates a new instance of <see cref="SpeedDictionary"/>.</summary>
        /// <param name="initialCapacity">The initial capacity of the list.</param>
        /// <param name="expansionMode">How to expand the list when it is full.</param>
        /// <param name="expansion">The value to increment or multiply the list capacity by when it is full.</param>
        public SpeedDictionary(int initialCapacity = 5, ExpansionMode expansionMode = ExpansionMode.Multiply, int expansionValue = 2)
        {
            _expansionMode = expansionMode;
            _expansionValue = expansionValue;

            Data = new T[initialCapacity];
            Keys = new K[initialCapacity];
            _itemCount = 0;

            _freeIDs = new int[5];
            _freeCount = 0;
        }

        /// <summary>Adds an item to the list, after checking whether or not a free ID is available to use instead of adding it as a new index to the end of the list.</summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The index of the item after adding it to the list.</returns>
        public int Add(K key, T item)
        {
            int id = _itemCount;

            //check for a free ID
            if (_freeCount > 0)
            {
                //pull the last ID off the free array
                _freeCount--;
                id = _freeIDs[_freeCount];
            }
            else
            {
                //make sure there is enough capacity to store the new item.
                if (_itemCount >= Data.Length)
                {
                    switch (_expansionMode)
                    {
                        case ExpansionMode.Multiply:
                            Array.Resize(ref Data, Data.Length * _expansionValue);
                            Array.Resize(ref Keys, Keys.Length * _expansionValue);
                            break;
                        case ExpansionMode.Increment:
                            Array.Resize(ref Data, Data.Length + _expansionValue);
                            Array.Resize(ref Keys, Keys.Length + _expansionValue);
                            break;
                    }
                }

                //A new item index was used, increment counter
                _itemCount++;
            }

            _actualCount++;

            Data[id] = item;
            Keys[id] = key;

            //return the ID
            return id;
        }

        /// <summary>Adds an item to the list, without checking for a free ID or whether the array has enough capacity. You must ensure this yourself.</summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The index of the item after adding it to the list.</returns>
        public int AddFast(K key, T item)
        {
            int id = _itemCount;
            _itemCount++;

            Data[id] = item;
            Keys[id] = key;

            //return the ID
            return id;
        }

        /// <summary>Removes the item at the specified index.</summary>
        /// <param name="index">The index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            int id = _freeCount;
            _freeCount++;

            if (_freeCount >= _freeIDs.Length)
            {
                switch (_expansionMode)
                {
                    case ExpansionMode.Multiply:
                        Array.Resize(ref _freeIDs, _freeIDs.Length * _expansionValue);
                        break;
                    case ExpansionMode.Increment:
                        Array.Resize(ref _freeIDs, _freeIDs.Length + _expansionValue);
                        break;
                }
            }

            _freeIDs[id] = index;
            _actualCount--;

            Data[index] = default(T);
            Keys[index] = default(K);
        }

        public void Remove(T item)
        {
            int index = Array.IndexOf(Data, item);

            if(index > -1)
                RemoveAt(index);
        }

        public void RemoveByKey(K key)
        {
            int index = Array.IndexOf(Keys, key);

            if (index > -1)
                RemoveAt(index);
            else
                throw new Exception("SpeedDictionary -- RemoveByKey: Key not found.");
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(Data, item, 0, _itemCount);
        }

        public int IndexOfKey(K key)
        {
            return Array.IndexOf(Keys, key, 0, _itemCount);
        }

        /// <summary>Expands the capacity of the list by the specified amount.</summary>
        /// <param name="amount">The amount to expand the list capacity. Negative numbers might cause problems.</param>
        public void Expand(int amount)
        {
            Array.Resize(ref Data, Data.Length + amount);
            Array.Resize(ref Keys, Data.Length);
        }

        /// <summary>Expands the capacity of the list by the specified amount.</summary>
        /// <param name="amount">The amount to expand the list capacity.</param>
        public void Shrink(int amount)
        {
            Array.Resize(ref Data, 
                Math.Max(0, Data.Length - amount));

            Array.Resize(ref Keys, Data.Length);
        }

        /// <summary>Returns true if the list contains the provided item already.</summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            return Data.Contains<T>(item);
        }

        /// <summary>Returns true if the SpeedTable contains the provided key.</summary>
        /// <param name="key">The key to search for.</param>
        /// <returns></returns>
        public bool ContainsKey(K key)
        {
            int index = Array.IndexOf(Keys, key);
            return index > -1;
        }

        /// <summary>Attempts to find the provided key and output the data it is tied to.</summary>
        /// <param name="key">The key to search for.</param>
        /// <param name="item">The location to output the result to if the key is found.</param>
        /// <returns></returns>
        public bool TryGetValue(K key, out T item)
        {
            bool found = false;
            item = default(T);

            int index = Array.IndexOf(Keys, key);

            if (index > -1)
                item = Data[index];

            return found;
        }

        /// <summary>Clears the contents of the list.</summary>
        public void Clear()
        {
            //wipe all items.
            for (int i = 0; i < _itemCount; i++)
            {
                Data[i] = default(T);
                Keys[i] = default(K);
            }

            _itemCount = 0;
            _freeCount = 0;
            _actualCount = 0;
        }

        /// <summary>Resets the list's position back to 0 without actually removing the data from the list.
        /// This is useful if you plan to overwrite the data in the list anyway.</summary>
        public void FastClear()
        {
            _itemCount = 0;
            _freeCount = 0;
            _actualCount = 0;
        }

        /// <summary>Ensures the speed list has at least enough space to store the number of items specified as capacity.
        /// If it doesn't, it will automatically resize its internal array to meet the requirements.</summary>
        /// <param name="capacity">The number of items you want the list to be able to store.</param>
        public void EnsureCapacity(int capacity)
        {
            //keep expanding by the expansion value until the required capacity is accounted for.
            while (Data.Length < capacity)
            {
                switch (_expansionMode)
                {
                    case ExpansionMode.Multiply:
                        Array.Resize(ref Data, Data.Length * _expansionValue);
                        Array.Resize(ref Keys, Data.Length * _expansionValue);
                        break;
                    case ExpansionMode.Increment:
                        Array.Resize(ref Data, Data.Length + _expansionValue);
                        Array.Resize(ref Keys, Data.Length + _expansionValue);
                        break;
                }
            }
        }

        /// <summary>Gets or sets the item at the specified index.</summary>
        /// <param name="index">The index of the item to read/write/manipulate.</param>
        /// <returns></returns>
        public T this[int index]
        {
            get { return Data[index]; }
            set { Data[index] = value; }
        }

        public T this[K key]
        {
            get
            {
                int index = Array.IndexOf(Keys, key);
                if (index > -1)
                    return Data[index];
#if DEBUG
                //if this code is reached, the key obviously wasn't detected.
                throw new Exception("SpeedDictionary: Key was not found.");
#else
                return default(T);
#endif
            }
        }

        /// <summary>Gets the last used list index. This does not mean all items between 0 and the returned value are valid objects. Some may be null.</summary>
        public int Length
        {
            get { return _itemCount; }
        }

        /// <summary>The actual number of items in the list, when null values are excluded. This is only updated when using Add and RemoveAt.</summary>
        public int ActualCount
        {
            get { return _actualCount; }
        }

        /// <summary>Gets the capacity of the list's internal item array.</summary>
        public int Capacity
        {
            get { return Data.Length; }
        }

        /// <summary>The capacity of the list of free IDs.</summary>
        public int FreeCount
        {
            get { return _freeCount; }
        }

        public int ExpansionValue
        {
            get { return _expansionValue; }
            set { _expansionValue = value; }
        }

        public ExpansionMode ExpansionMode
        {
            get { return _expansionMode; }
            set { _expansionMode = value; }
        }
    }
}
