using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{

    public partial class TextureData
    {
        public unsafe class SliceRef<T>
            where T: unmanaged
        {
            Slice _slice;

            internal SliceRef(Slice slice)
            {
                _slice = slice;
            }

            public T* this[uint x, uint y] => ((T*)_slice.Data) + _slice.ElementsPerPixel * (_slice.Width * y + x);
        }

        /// <summary>Represents a slice of texture data. This can either be a mip map level or array element in a texture array (which could still technically a mip-map level of 0).</summary>
        public unsafe class Slice
        {
            byte* _data;

            public byte* Data => _data;

            public uint ElementsPerPixel;
            public uint Pitch;
            public uint TotalBytes { get; private set; }

            public uint Width;
            public uint Height;

            public Slice(uint numBytes)
            {
                Allocate(numBytes);
            }

            public SliceRef<T> GetReference<T>() 
                where T: unmanaged
            {
                return new SliceRef<T>(this);
            }

            public void Allocate(uint numBytes)
            {
                if (Data != null)
                    EngineUtil.Free(ref _data);

                TotalBytes = numBytes;
                _data = (byte*)EngineUtil.Alloc(numBytes);
            }

            ~Slice()
            {
                if (Data != null)
                    EngineUtil.Free(ref _data);
            }

            public Slice Clone()
            {
                Slice result = new Slice(TotalBytes)
                {
                    Pitch = Pitch,
                    TotalBytes = TotalBytes,
                    Width = Width,
                    Height = Height,
                };

                Buffer.MemoryCopy(_data, result._data, TotalBytes, TotalBytes);

                return result;
            }
        }
    }
}
