using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class ResourceStream
    {
        MappedSubresource _mapping;
        Map _mapType;

        internal long Position { get; set; }

        uint _rowPitch;
        uint _depthPitch;
        bool _canWrite;
        bool _canRead;

        internal ResourceStream(MappedSubresource mapping, Map mapType)
        {
            _mapping = mapping;
            _mapType = mapType;
            _canWrite = !((_mapType & Map.MapRead) == Map.MapRead);
            _canRead = !((_mapType & Map.MapRead) == Map.MapRead ||
                (_mapType & Map.MapReadWrite) == Map.MapReadWrite);
        }

        public void Write(byte[] bytes)
        {
            fixed (byte* ptr = bytes)
                Write(ptr, (uint)bytes.Length);
        }

        public void Write(byte* bytes, uint numBytes)
        {
            if (!_canWrite)
                throw new ResourceStreamException(_mapType, $"Map mode does not allow writing.");

            Buffer.MemoryCopy(bytes, _mapping.PData, numBytes, numBytes);
            Position += numBytes;
        }

        public void Write<T>(T[] value) where T : unmanaged
        {
            if (!_canWrite)
                throw new ResourceStreamException(_mapType, $"Map mode does not allow writing.");

            long numBytes = sizeof(T) * value.Length;
            EngineInterop.PinObject(value, (p) =>
            {
                void* ptr = p.ToPointer();
                Buffer.MemoryCopy(ptr, _mapping.PData, numBytes, numBytes);
            });

            Position += numBytes;
        }

        public void Write<T>(T* value, uint numElements) where T : unmanaged
        {
            if (!_canWrite)
                throw new ResourceStreamException(_mapType, $"Map mode does not allow writing.");

            long numBytes = sizeof(T) * numElements;
            Buffer.MemoryCopy(value, _mapping.PData, numBytes, numBytes);
            Position += numBytes;
        }
    }
}
