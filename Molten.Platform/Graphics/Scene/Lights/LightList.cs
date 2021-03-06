﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// A list 
    /// </summary>
    public class LightList
    {
        public LightInstance[] Instances;
        public LightData[] Data;
        int[] _free;
        int _freeCount;
        int _elementCount; // Number of elements initialized at least once.
        int _itemCount; // Number of active items.
        int _resizeIncrement;

        internal LightList(int initialCapacity, int resizeIncrement)
        {
            Data = new LightData[initialCapacity];
            Instances = new LightInstance[initialCapacity];
            _free = new int[10];
            _resizeIncrement = resizeIncrement;
            _itemCount = 0;
        }

        public void EnsureCapacity(int capacity)
        {
            if (capacity >= Data.Length)
                Array.Resize(ref Data, capacity);
        }

        public LightInstance New(LightData data)
        {
            int id = 0;
            if(_freeCount > 0)
            {
                id = _free[--_freeCount];
            }
            else
            {
                id = _elementCount++;
                if (_elementCount == Data.Length)
                {
                    Array.Resize(ref Data, Data.Length + _resizeIncrement);
                    Array.Resize(ref Instances, Data.Length);
                }
            }

            Data[id] = new LightData();
            Instances[id] = Instances[id] ?? new LightInstance() { ID = id };
            _itemCount++;
            return Instances[id];
        }

        public void Remove(LightInstance instance)
        {
            Data[instance.ID].TessFactor = 0; // Lights with a tess factor of 0 will be skipped.

            if (_freeCount == _free.Length)
                Array.Resize(ref _free, _free.Length * 2);

            if (instance.ID == _elementCount - 1)
                _elementCount--;
            else
                _free[_freeCount++] = instance.ID;

            _itemCount--;
        }

        public int ItemCount => _itemCount;

        public int ElementCount => _elementCount;

        public int ResizeIncrement => _resizeIncrement;
    }

    /// <summary>
    /// Represents a light instance of any type. Acts as a container for extra data that is only needed CPU-side for preparation purposes, to avoid wasting valuable GPU bandwidth.
    /// </summary>
    public class LightInstance
    {
        public float Range;
        public int ID;
    }
}
