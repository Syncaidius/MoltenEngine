using System.Runtime.CompilerServices;

namespace Molten.IO
{
    /// <summary>
    /// A <see cref="Stream"/> implementation for writing to a block of unsafe, fixed-size memory.
    /// </summary>
    public unsafe class RawStream : Stream
    {
        long _pos;
        long _length;
        byte* _ptrDataStart;
        byte* _ptrData;
        bool _canRead;
        bool _canWrite;

        public RawStream(void* ptrData, uint numBytes, bool canRead, bool canWrite)
        {
            SetSource(ptrData, numBytes, canRead, canWrite);
        }

        /// <summary>
        /// Sets the data source of the current <see cref="RawStream"/>.
        /// </summary>
        /// <remarks>Resets <see cref="Position"/> to zero and updates <see cref="Length"/>.</remarks>
        /// <param name="ptrData">A pointer to the source data.</param>
        /// <param name="numBytes">The number of bytes that <paramref name="ptrData"/> represents</param>.
        /// <param name="canRead"></param>
        /// <param name="canWrite"></param>
        public void SetSource(void* ptrData, uint numBytes, bool canRead, bool canWrite)
        {
            _ptrData = (byte*)ptrData;
            _ptrDataStart = _ptrData;
            _canRead = canRead;
            _canWrite = canWrite;
            _length = numBytes;
        }

        public override void SetLength(long value)
        {
            if (_length <= 0)
                throw new RawStreamException(this, "Length must be greater than zero.");

            if (_length < _pos)
                throw new RawStreamException(this, "Length cannot be less than the current position.");

            _length = value;
        }

        public void Write<T>(T* ptrValue, uint numElements = 1) where T : unmanaged
        {
            Write(ptrValue, sizeof(T) * numElements);
        }

        public void Write<T>(T value) where T : unmanaged
        {
            Write(ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(ref T value) where T : unmanaged
        {
            if (!CanWrite)
                throw new RawStreamException(this, $"Map mode does not allow writing.");

            ((T*)_ptrData)[0] = value;
            Position += sizeof(T);
        }

        public void Write<T>(T[] values, uint offset, uint numElements) where T : unmanaged
        {
            fixed (T* ptrValues = values)
            {
                T* p = ptrValues + offset;
                Write(p, numElements);
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            fixed (byte* ptr = buffer)
            {
                byte* p = ptr + offset;
                Write(p, (uint)count);
            }
        }

        public void Write(byte[] bytes)
        {
            fixed (byte* ptr = bytes)
                Write(ptr, bytes.LongLength);
        }

        public void Write(void* pData, long numBytes)
        {
            if (!CanWrite)
                throw new RawStreamException(this, $"Map mode does not allow writing.");

            Buffer.MemoryCopy(pData, _ptrData, numBytes, numBytes);
            Position += numBytes;
        }

        /// <summary>
        /// Writes an array of values to the current <see cref="RawStream"/>.
        /// </summary>
        /// <typeparam name="T">The type of element to write.</typeparam>
        /// <param name="values">The array of values from which to write to the current <see cref="RawStream"/>.</param>
        /// <param name="startIndex">The start index within <paramref name="values"/> to start copying.</param>
        /// <param name="count">The number of elements to write from <paramref name="values"/> to the current <see cref="RawStream"/>.</param>
        /// <exception cref="RawStreamException"></exception>
        public void WriteRange<T>(T[] values, uint startIndex, uint count)
            where T : unmanaged
        {
            long numBytes = sizeof(T) * count;

            fixed (T* ptr = &values[startIndex])
                WriteRange(ptr, numBytes);
        }

        public void WriteRange(void* ptrData, long numBytes)
        {
            if (!CanWrite)
                throw new RawStreamException(this, $"Map mode does not allow writing.");

            Buffer.MemoryCopy(ptrData, _ptrData, numBytes, numBytes);
            Position += numBytes;
        }

        /// <summary>
        /// Reads an array of values from the current <see cref="RawStream"/>.
        /// </summary>
        /// <typeparam name="T">The type of element to read.</typeparam>
        /// <param name="destination">The destination array to which values will be read into.</param>
        /// <param name="startIndex">The index at which to place the first copied element within the <paramref name="destination"/>.</param>
        /// <param name="count">The number of elements to read from the current <see cref="RawStream"/>.</param>
        /// <exception cref="RawStreamException"></exception>
        public void ReadRange<T>(T[] destination, uint startIndex, uint count)
            where T : unmanaged
        {
            if (!CanRead)
                throw new RawStreamException(this, $"Map mode does not allow reading.");

            long numBytes = sizeof(T) * count;
            fixed(T* ptr = &destination[startIndex])
                Buffer.MemoryCopy(_ptrData, ptr, numBytes, numBytes);

            Position += numBytes;
        }

        public T Read<T>() where T : unmanaged
        {
            T* tmp = stackalloc T[1];
            Buffer.MemoryCopy(_ptrData, tmp, sizeof(T), sizeof(T));
            Position += sizeof(T);
            return tmp[0];
        }

        public void Read<T>(ref T dest) where T : unmanaged
        {
            T* tmp = stackalloc T[1];
            Buffer.MemoryCopy(_ptrData, tmp, sizeof(T), sizeof(T));
            Position += sizeof(T);
            dest = tmp[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="destination"></param>
        /// <param name="offset">Offset in destination array to copy to.</param>
        /// <param name="numElements"></param>
        public void Read<T>(T[] destination, uint offset, uint numElements) where T : unmanaged
        {
            long numBytes = sizeof(T) * numElements;
            fixed (T* ptrDest = destination)
            {
                T* p = ptrDest + offset;
                Buffer.MemoryCopy(_ptrData, p, numBytes, numBytes);
            }

            Position += sizeof(T) * numElements;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            fixed(byte* ptrBuffer = buffer)
            {
                byte* pBuffer = ptrBuffer + offset;
                Buffer.MemoryCopy(_ptrData, pBuffer, count, count);
            }

            Position += count;
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;

                case SeekOrigin.End:
                    Position = _length - offset;
                    break;

                case SeekOrigin.Current:
                    Position = _pos + offset;
                    break;
            }

            return _pos;
        }

        public override void Flush()
        {
            // TODO implement optional buffering? (which would need flushing)
        }

        /// <summary>
        /// Gets or sets the stream position.
        /// </summary>
        public override long Position
        {
            get => _pos;
            set
            {
                if (value > _length && _pos > 0)
                    throw new RawStreamException(this, $"Position cannot be less than zero or greater than the stream length.");

                _pos = value;
                _ptrData = _ptrDataStart + _pos;
            }
        }

        /// <summary>
        /// Gets the underlying data pointer.
        /// </summary>
        public void* DataPtr => _ptrDataStart;

        /// <summary>
        /// Gets whether or not the stream can seek using <see cref="Seek(long, SeekOrigin)"/>.
        /// </summary>
        public override bool CanSeek => true;

        /// <summary>
        /// Gets whether or not the stream can perform read operations.
        /// </summary>
        public override bool CanRead => _canRead;

        /// <summary>
        /// Gets whether or not the stream can perform write operations.
        /// </summary>
        public override bool CanWrite => _canWrite;

        /// <summary>
        /// Gets whether or not the stream will timeout.
        /// </summary>
        public override bool CanTimeout => false;

        /// <summary>
        /// Gets the length of the stream's source/target data.
        /// </summary>
        public override long Length => _length;
    }
}
