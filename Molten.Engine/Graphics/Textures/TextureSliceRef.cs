using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract unsafe class TextureSliceRef
    {
        internal abstract void UpdateReference();
    }

    public unsafe class TextureSliceRef<T> : TextureSliceRef
        where T : unmanaged
    {
        TextureSlice _slice;
        T* _refData;

        internal TextureSliceRef(TextureSlice slice)
        {
            _slice = slice;
            UpdateReference();
        }

        internal override void UpdateReference()
        {
            _refData = (T*)_slice.Data;
        }

        public T this[uint p] => _refData[p];

        public T this[int p] => _refData[p];

        public T* this[uint x, uint y] => _refData + _slice.ElementsPerPixel * (_slice.Width * y + x);

        public T* this[int x, int y] => _refData + _slice.ElementsPerPixel * (_slice.Width * y + x);

        public uint ElementsPerPixel => _slice.ElementsPerPixel;

        public T* Data => _refData;

        public uint Width => _slice.Width;

        public uint Height => _slice.Height;
    }
}
