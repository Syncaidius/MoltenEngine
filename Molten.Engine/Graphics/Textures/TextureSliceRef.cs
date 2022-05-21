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
        T* _refData;

        internal TextureSliceRef(TextureSlice slice)
        {
            Slice = slice;
            UpdateReference();
        }

        internal override void UpdateReference()
        {
            _refData = (T*)Slice.Data;
        }

        public ref T this[uint p] => ref _refData[p];

        public ref T this[int p] => ref _refData[p];

        public T* this[uint x, uint y] => _refData + (Slice.Width * y + x);

        public T* this[int x, int y] => _refData + (Slice.Width * y + x);

        public T* Data => _refData;

        public uint Width => Slice.Width;

        public uint Height => Slice.Height;

        public TextureSlice Slice { get; }
    }
}
