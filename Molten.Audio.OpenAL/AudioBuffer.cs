using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Audio.OpenAL
{
    public unsafe class AudioBuffer : IAudioBuffer
    {
        /// <summary>
        /// Invoked when the current <see cref="IAudioBuffer"/> is about to be disposed.
        /// </summary>
        public event ObjectHandler<IAudioBuffer> OnDisposing;

        /// <summary>
        /// Invoked when the current <see cref="IAudioBuffer"/> has been disposed.
        /// </summary>
        public event ObjectHandler<IAudioBuffer> OnDisposed;

        byte* _data;
        uint _writePosition;
        uint _readPosition;

        /// <summary>
        /// Creates a new instance of <see cref="AudioBuffer"/>.
        /// </summary>
        /// <param name="bufferSize">The buffer size, in bytes.</param>
        internal AudioBuffer(uint bufferSize)
        {
            Size = bufferSize;
            _data = EngineUtil.AllocArray<byte>(bufferSize);
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

        public void Dispose()
        {
            if (_data != null)
            {
                OnDisposing?.Invoke(this);
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

        internal byte* PtrStart => _data;

        /// <summary>
        /// Gets <see cref="PtrStart"/> offset by <see cref="ReadPosition"/>.
        /// </summary>
        internal byte* PtrRead => _data + ReadPosition;

        /// <summary>
        /// Gets <see cref="PtrStart"/> offset by <see cref="WritePosition"/>.
        /// </summary>
        internal byte* PtrWrite => _data + WritePosition;
    }
}
