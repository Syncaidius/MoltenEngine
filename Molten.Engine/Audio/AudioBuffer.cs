using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Audio
{
    public unsafe class AudioBuffer : EngineObject
    {
        /// <summary>
        /// Invoked when the current <see cref="IAudioBuffer"/> has been disposed.
        /// </summary>
        public event ObjectHandler<AudioBuffer> OnDisposed;

        byte* _data;
        uint _writePosition;
        uint _readPosition;

        /// <summary>
        /// Creates a new instance of <see cref="AudioBuffer"/>.
        /// </summary>
        /// <param name="bufferSize">The buffer size, in bytes.</param>
        internal AudioBuffer(uint bufferSize, AudioFormat format, uint frequency)
        {
            Size = bufferSize;
            Frequency = frequency;
            Format = format;
            _data = EngineUtil.AllocArray<byte>(bufferSize);
        }

        public unsafe uint Read(byte[] buffer)
        {
            fixed (byte* ptr = buffer)
                return Read(ptr, (uint)buffer.Length);
        }

        public unsafe uint Write(byte[] data)
        {
            fixed (byte* ptr = data)
                return Write(ptr, (uint)data.Length);
        }

        public uint Read(byte* buffer, uint numBytes)
        {
            uint remaining = Size - ReadPosition;
            numBytes = Math.Min(numBytes, remaining);

            Buffer.MemoryCopy(PtrRead, buffer, numBytes, numBytes);
            ReadPosition += numBytes;
            return numBytes;
        }

        public uint Write(byte* data, uint numBytes)
        {
            uint remaining = Size - WritePosition;
            numBytes = Math.Min(numBytes, remaining);

            Buffer.MemoryCopy(data, PtrWrite, numBytes, numBytes);
            WritePosition += numBytes;

            return numBytes;
        }

        protected override void OnDispose()
        {
            if (_data != null)
            {
                EngineUtil.Free(ref _data);
                OnDisposed?.Invoke(this);
            }
        }

        public byte this[uint position]
        {
            get => _data[position];
            set => _data[position] = value;
        }

        public byte this[int position]
        {
            get => _data[position];
            set => _data[position] = value;
        }

        public uint WritePosition
        {
            get => _writePosition;
            set
            {
                if (_writePosition > Size)
                    throw new Exception("The position cannot exceed the size of the buffer");

                _writePosition = value;
            }
        }

        public uint ReadPosition
        {
            get => _readPosition;
            set
            {
                if (_readPosition > Size)
                    throw new Exception("The position cannot exceed the size of the buffer");

                _readPosition = value;
            }
        }

        public uint Size { get; }

        public byte* PtrStart => _data;

        /// <summary>
        /// Gets <see cref="PtrStart"/> offset by <see cref="ReadPosition"/>.
        /// </summary>
        public byte* PtrRead => _data + ReadPosition;

        /// <summary>
        /// Gets <see cref="PtrStart"/> offset by <see cref="WritePosition"/>.
        /// </summary>
        public byte* PtrWrite => _data + WritePosition;

        public AudioFormat Format { get; }

        public uint Frequency { get; }
    }
}
