using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.IO
{
    public unsafe class RawStream : Stream
    {
        long _pos;
        long _length;
        byte* _ptrDataStart;
        byte* _ptrData;

        internal RawStream(void* ptrData, bool canRead, bool canWrite)
        {
            _ptrDataStart = (byte*)ptrData;
            CanRead = canRead;
            CanWrite = canWrite;
            _length = 0;
        }

        public override void SetLength(long value)
        {
            if (_length <= 0)
                throw new RawStreamException(this, "Length must be greater than zero.");

            if (_pos > _length)
                _pos = _length;

            _length = value;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            fixed (byte* ptr = buffer)
            {
                byte* p = ptr;
                p += offset;
                Write(p, (uint)count);
            }
        }

        public void Write(byte[] bytes)
        {
            fixed (byte* ptr = bytes)
                Write(ptr, (uint)bytes.Length);
        }

        public void Write(byte* bytes, uint numBytes)
        {
            if (!CanWrite)
                throw new RawStreamException(this, $"Map mode does not allow writing.");

            Buffer.MemoryCopy(bytes, _ptrDataStart, numBytes, numBytes);
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

            fixed(T* ptr = values)
            {
                T* p = ptr;
                p += startIndex;
                WriteRange(p, numBytes);
            }
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
            fixed(T* ptr = destination)
            {
                T* p = ptr;
                p += startIndex;

                Buffer.MemoryCopy(_ptrData, p, numBytes, numBytes);
            }

            Position += numBytes;
        }

        public T Read<T>() where T : unmanaged
        {
            T* tmp = stackalloc T[1];
            Buffer.MemoryCopy(_ptrData, tmp, sizeof(T), sizeof(T));
            Position += sizeof(T);
            return tmp[0];
        }

        public void Read<T>(T[] destination, uint numElements) where T : unmanaged
        {
            long numBytes = sizeof(T) * numElements;
            fixed(T* ptrDest = destination)
                Buffer.MemoryCopy(_ptrData, ptrDest, numBytes, numBytes);

            Position += sizeof(T);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            fixed(byte* ptrBuffer = buffer)
            {
                byte* pBuffer = ptrBuffer;
                pBuffer += offset;
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
                    _pos += offset;
                    if (_pos > _length)
                        _length = _pos;
                    break;

                case SeekOrigin.End:
                    long ePos = _length - offset;
                    if (ePos < 0)
                        throw new RawStreamException(this, $"The position ({ePos}) would seek below zero.");

                    _pos = ePos;
                    if (_pos > _length)
                        _length = _pos;
                    break;

                case SeekOrigin.Current:
                    long cPos = _pos + offset;
                    if(cPos < 0)
                        throw new RawStreamException(this, $"The position ({cPos}) would seek below zero.");

                    _pos = cPos;
                    if (_pos > _length)
                        _length = _pos;
                    break;
            }

            _ptrData = _ptrDataStart + _pos;
            return _pos;
        }

        public override void Flush()
        {
            // TODO implement optional buffering? (which would need flushing)
        }

        public override long Position
        {
            get => _pos;
            set
            {
                if (CanWrite)
                {
                    _pos = value;
                    if (_pos > _length)
                        _length = _pos;
                }
                else
                {
                    if (value > _length)
                        throw new RawStreamException(this, $"Position cannot be outside stream length ({_length}).");
                    else
                        _pos = value;
                }

                _ptrData = _ptrDataStart + _pos;
            }
        }

        public override bool CanSeek => true;

        public override bool CanRead { get; }

        public override bool CanWrite { get; }

        public override bool CanTimeout => false;

        public override long Length => _length;
    }
}