using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Molten.Collections
{
    /// <summary>A List-esque collection class without all the baggage and frills of a generic collection List. Works like a hybrid of a list and an array.</summary>
    /// <typeparam name="T">The type of object you want the list to store.</typeparam>
    public class SpeedList<T>
    {
        /// <summary>The raw array containing the contents of the list. Accessing the items held within the list via this 
        /// array will be significantly faster, but unmanaged.</summary>
        public T[] Data;
        private int _itemCount;
        private int _actualCount;

        private int[] _freeIDs;
        private int _freeCount;

        ExpansionMode _expansionMode;
        int _expansionValue;

        T _defaultValue = default(T);

        /// <summary>Creates a new instance of SpeedList</summary>
        /// <param name="initialCapacity">The initial capacity of the list.</param>
        /// <param name="expansionMode">How to expand the list when it is full.</param>
        /// <param name="expansion">The value to increment or multiply the list capacity by when it is full.</param>
        public SpeedList(int initialCapacity = 5, ExpansionMode expansionMode = ExpansionMode.Multiply, int expansionValue = 2)
        {
            _expansionMode = expansionMode;
            _expansionValue = expansionValue;

            Data = new T[initialCapacity];
            _itemCount = 0;

            _freeIDs = new int[5];
            _freeCount = 0;
        }

        /// <summary>Adds an item to the list only if it isn't already part of it. However, this method is slower than Add() or AddFast()</summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The ID of the item if it was added, or its existing ID if it was previously added.</returns>
        public int AddSafe(T item)
        {
            //check if the item is already in the data array.
            int index = Array.IndexOf(Data, item);

            //if not, add it. If it already exists, return its existing ID.
            if (index == -1)
                return Add(item);
            else
                return index;
        }

        /// <summary>Adds an item to the list, after checking whether or not a free ID is available to use instead of adding it as a new index to the end of the list.</summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The index of the item after adding it to the list.</returns>
        public int Add(T item)
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
                            break;
                        case ExpansionMode.Increment:
                            Array.Resize(ref Data, Data.Length + _expansionValue);
                            break;
                    }
                }

                //A new item index was used, increment counter
                _itemCount++;
            }

            _actualCount++;

            Data[id] = item;

            //return the ID
            return id;
        }

        /// <summary>Adds an item to the list, without checking for a free ID or whether the array has enough capacity. You must ensure this yourself.</summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The index of the item after adding it to the list.</returns>
        public int AddFast(T item)
        {
            int id = _itemCount;
            _itemCount++;
            _actualCount++;

            Data[id] = item;

            //return the ID
            return id;
        }

        /// <summary>Adds all items in the given collection to the list.</summary>
        /// <param name="collection">The collection of items to add to the list.</param>
        public void AddRange(IEnumerable<T> collection)
        {
            foreach (T item in collection)
                Add(item);
        }

        /// <summary>Adds all items in the given collection to the end of the list, bypassing checks on free index IDs.</summary>
        /// <param name="collection">The collection of items to add to the list.</param>
        public void AddRangeFast(IEnumerable<T> collection)
        {
            foreach (T item in collection)
                AddFast(item);
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

            //reverse the item count if the index was the last item.
            if (index == _itemCount - 1)
                _itemCount--;

            _freeIDs[id] = index;
            _actualCount--;

            Data[index] = _defaultValue;
        }

        public void Remove(T item)
        {
            int index = Array.IndexOf(Data, item);

            if(index > -1)
                RemoveAt(index);
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(Data, item);
        }

        /// <summary>Expands the capacity of the list by the specified amount.</summary>
        /// <param name="amount">The amount to expand the list capacity. Negative numbers might cause problems.</param>
        public void Expand(int amount)
        {
            Array.Resize(ref Data, Data.Length + amount);
        }

        /// <summary>Expands the capacity of the list by the specified amount.</summary>
        /// <param name="amount">The amount to expand the list capacity.</param>
        public void Shrink(int amount)
        {
            Array.Resize(ref Data, 
                Math.Max(0, Data.Length - amount));
        }

        /// <summary>Returns true if the list contains the provided item already.</summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            return Data.Contains<T>(item);
        }

        /// <summary>Clears the contents of the list.</summary>
        public void Clear()
        {
            //wipe all items.
            for (int i = 0; i < _itemCount; i++)
                Data[i] = _defaultValue;

            _itemCount = 0;
            _freeCount = 0;
            _actualCount = 0;
        }

        /// <summary>Resets the list's position back to 0 without actually removing the data from the list.
        /// This is useful if you plan to overwrite the data in the list anyway.</summary>
        public void ClearFast()
        {
            _itemCount = 0;
            _freeCount = 0;
            _actualCount = 0;
        }

        public T[] GetArray()
        {
            return Data;
        }

        /// <summary>Converts a speed list into an array.</summary>
        /// <param name="list">The list to convert.</param>
        /// <returns></returns>
        public static implicit operator T[](SpeedList<T> list)
        {
            return list.Data;
        }

        public override string ToString()
        {
            return _itemCount.ToString();
        }

        /// <summary>Ensures the speed list has at least enough space to store the number of items specified as capacity.
        /// If it doesn't, it will automatically resize its internal array to meet the requirements.</summary>
        /// <param name="capacity">The number of items you want the list to be able to store.</param>
        public void EnsureCapacity(int capacity)
        {
            if (Data.Length < capacity)
                Array.Resize(ref Data, capacity);
        }

        /// <summary>Creates an indentical clone of the SpeedList and returns the cloned copy.</summary>
        /// <returns></returns>
        public SpeedList<T> Clone()
        {
            SpeedList<T> newList = new SpeedList<T>();
            newList.Data = new T[Data.Length];
            newList._itemCount = _itemCount;
            newList._freeIDs = new int[_freeIDs.Length];
            newList._freeCount = _freeCount;
            newList._expansionMode = _expansionMode;
            newList._expansionValue = _expansionValue;

            //copy item data over to the clone
            Array.Copy(Data, newList.Data, Data.Length);
            Array.Copy(_freeIDs, newList._freeIDs, _freeIDs.Length);

            return newList;
        }

        /// <summary>Copies the data of the current list, to another list, overwriting all previous data in the other list.</summary>
        /// <param name="otherList">The other speed list to copy the data to.</param>
        public void CloneTo(ref SpeedList<T> otherList)
        {
            if (otherList.Data.Length != Data.Length)
                Array.Resize(ref otherList.Data, Data.Length);

            Array.Copy(Data, otherList.Data, Data.Length);

            otherList._itemCount = _itemCount;
            otherList._freeCount = 0;
        }

        /// <summary>Gets or sets the item at the specified index.</summary>
        /// <param name="index">The index of the item to read/write/manipulate.</param>
        /// <returns></returns>
        public T this[int index]
        {
            get { return Data[index]; }
            set { Data[index] = value; }
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

        /// <summary>The value that list elements are reset to when being removed, reset or cleared..</summary>
        public T DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }
    }
}
