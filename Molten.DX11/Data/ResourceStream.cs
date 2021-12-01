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

        /// <summary>
        /// Writes an array of values to the current <see cref="ResourceStream"/>.
        /// </summary>
        /// <typeparam name="T">The type of element to write.</typeparam>
        /// <param name="value"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <exception cref="ResourceStreamException"></exception>
        public void WriteRange<T>(T[] value, uint startIndex, uint count)
            where T : struct
        {
            if (!_canWrite)
                throw new ResourceStreamException(_mapType, $"Map mode does not allow writing.");

            int sizeOf = Marshal.SizeOf<T>();
            long numBytes = sizeOf * count;
            int byteOffset = (int)startIndex * sizeOf;

            EngineInterop.PinObject(value, (p) =>
            {
                p += byteOffset;
                void* ptr = p.ToPointer();
                Buffer.MemoryCopy(ptr, _mapping.PData, numBytes, numBytes);
            });

            Position += numBytes;
        }

        /// <summary>
        /// Writes an array of values to the current <see cref="ResourceStream"/>.
        /// </summary>
        /// <typeparam name="T">The type of element to read.</typeparam>
        /// <param name="destination">The destination array to which values will be read into.</param>
        /// <param name="startIndex">The index at which to place the first copied element within the <paramref name="destination"/>.</param>
        /// <param name="count">The number of elements to read from the current <see cref="ResourceStream"/>.</param>
        /// <exception cref="ResourceStreamException"></exception>
        public void ReadRange<T>(T[] destination, uint startIndex, uint count)
            where T : struct
        {
            if (!_canWrite)
                throw new ResourceStreamException(_mapType, $"Map mode does not allow writing.");

            int sizeOf = Marshal.SizeOf<T>();
            long numBytes = sizeOf * count;
            int byteOffset = (int)startIndex * sizeOf;

            EngineInterop.PinObject(destination, (p) =>
            {
                p += byteOffset;
                void* ptr = p.ToPointer();
                Buffer.MemoryCopy(_mapping.PData, ptr, numBytes, numBytes);
            });

            Position += numBytes;
        }
    }
}