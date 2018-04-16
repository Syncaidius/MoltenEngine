using System;
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
        public LightData[] Data;
        int[] _free;
        int _freeCount;
        int _elementCount; // Number of elements initialized at least once.
        int _itemCount; // Number of active items.
        int _resizeIncrement;

        internal LightList(int initialCapacity, int resizeIncrement)
        {
            Data = new LightData[initialCapacity];
            _free = new int[10];
            _resizeIncrement = resizeIncrement;
            _itemCount = 0;
        }

        public void EnsureCapacity(int capacity)
        {
            if (capacity >= Data.Length)
                Array.Resize(ref Data, capacity);
        }

        public int Add(LightData data)
        {
            int id = 0;
            if(_freeCount > 0)
            {
                id = _free[--_freeCount];
            }
            else
            {
                id = _elementCount++;
                if(_elementCount == Data.Length)
                    Array.Resize(ref Data, Data.Length + _resizeIncrement);
            }

            Data[id] = data;
            _itemCount++;
            return id;
        }

        public void Remove(int id)
        {
            Data[id].TessFactor = 0; // Lights with a tess factor of 0 will be skipped.

            if (_freeCount == _free.Length)
                Array.Resize(ref _free, _free.Length * 2);

            if (id == _elementCount - 1)
                _elementCount--;
            else
                _free[_freeCount++] = id;

            _itemCount--;
        }

        public int ItemCount => _itemCount;

        public int ElementCount => _elementCount;

        public int ResizeIncrement => _resizeIncrement;
    }
}
