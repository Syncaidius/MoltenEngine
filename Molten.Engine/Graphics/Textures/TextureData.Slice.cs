using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{

    public partial class TextureData
    {
        public abstract unsafe class SliceRef
        {
            internal abstract void UpdateReference();
        }

        public unsafe class SliceRef<T> : SliceRef
            where T: unmanaged
        {
            Slice _slice;
            T* _refData;

            internal SliceRef(Slice slice)
            {
                _slice = slice;
                UpdateReference();
            }

            internal override void UpdateReference()
            {
                _refData = (T*)_slice.Data;
            }

            public T* this[uint x, uint y] => _refData + _slice.ElementsPerPixel * (_slice.Width * y + x);
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

            List<SliceRef> _references;

            public Slice(uint numBytes)
            {
                Allocate(numBytes);
            }

            public Slice(byte[] data, uint numBytes)
            {
                Allocate(numBytes);

                fixed (byte* ptrData = data)
                    Buffer.MemoryCopy(ptrData, Data, numBytes, numBytes);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T">The reference data type.</typeparam>
            /// <returns></returns>
            public SliceRef<T> GetReference<T>() 
                where T: unmanaged
            {
                SliceRef<T> sr = new SliceRef<T>(this);
                _references.Add(sr);
                return sr;
            }

            public void Allocate(uint numBytes)
            {
                if (Data != null)
                    EngineUtil.Free(ref _data);

                TotalBytes = numBytes;
                _data = (byte*)EngineUtil.Alloc(numBytes);
                foreach (SliceRef sr in _references)
                    sr.UpdateReference();
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
