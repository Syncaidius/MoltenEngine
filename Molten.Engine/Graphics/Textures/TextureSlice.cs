using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Represents a slice of texture data. This can either be a mip map level or array element in a texture array (which could still technically a mip-map level of 0).</summary>
    public unsafe class TextureSlice : IDisposable
    {
        byte* _data;

        public byte* Data => _data;

        public uint ElementsPerPixel;
        public uint Pitch;
        public uint TotalBytes { get; private set; }

        public uint Width { get; private set; }
        public uint Height { get; private set; }

        List<TextureSliceRef> _references = new List<TextureSliceRef>();

        public TextureSlice(uint width, uint height, uint numBytes)
        {
            Width = width;
            Height = height;
            Allocate(numBytes);
        }

        public TextureSlice(uint width, uint height, byte* data, uint numBytes)
        {
            Width = width;
            Height = height;
            _data = data;
            TotalBytes = numBytes;
        }

        public TextureSlice(uint width, uint height, byte[] data, uint startIndex, uint numBytes)
        {
            Width = width;
            Height = height;
            Allocate(numBytes);

            fixed (byte* ptrData = data)
            {
                byte* ptr = ptrData + startIndex;
                Buffer.MemoryCopy(ptr, Data, numBytes, numBytes);
            }
        }

        public TextureSlice(uint width, uint height, byte[] data)
        {
            Width = width;
            Height = height;
            uint numBytes = (uint)data.Length;

            Allocate(numBytes);

            fixed (byte* ptrData = data)
                Buffer.MemoryCopy(ptrData, Data, numBytes, numBytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">The reference data type.</typeparam>
        /// <returns></returns>
        public TextureSliceRef<T> GetReference<T>()
            where T : unmanaged
        {
            TextureSliceRef<T> sr = new TextureSliceRef<T>(this);
            _references.Add(sr);
            return sr;
        }

        public void Allocate(uint numBytes)
        {
            if (Data != null)
                EngineUtil.Free(ref _data);

            TotalBytes = numBytes;
            _data = (byte*)EngineUtil.Alloc(numBytes);
            foreach (TextureSliceRef sr in _references)
                sr.UpdateReference();
        }

        ~TextureSlice()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Data != null)
                EngineUtil.Free(ref _data);
        }

        public TextureSlice Clone()
        {
            TextureSlice result = new TextureSlice(Width, Height, TotalBytes)
            {
                Pitch = Pitch,
                TotalBytes = TotalBytes,
            };

            Buffer.MemoryCopy(_data, result._data, TotalBytes, TotalBytes);

            return result;
        }
    }
}
