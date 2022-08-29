using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Audio
{
    public interface IAudioBuffer : IDisposable
    {
        event ObjectHandler<IAudioBuffer> OnDisposing;

        event ObjectHandler<IAudioBuffer> OnDisposed;


        unsafe uint Read(byte[] buffer)
        {
            fixed (byte* ptr = buffer)
                return Read(ptr, (uint)buffer.Length);
        }

        unsafe uint Read(byte* buffer, uint numBytes);

        unsafe uint Write(byte[] data)
        {
            fixed (byte* ptr = data)
                return Write(ptr, (uint)data.Length);
        }

        unsafe uint Write(byte* data, uint numBytes);

        /// <summary>
        /// Gets or sets the byte at the given buffer position, without affecting <see cref="Position"/>.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        byte this[uint position] { get; set; }

        /// <summary>
        /// Gets or sets the byte at the given buffer position, without affecting <see cref="Position"/>.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        byte this[int position] { get; set; }

        /// <summary>
        /// Gets the size of the buffer, in bytes.
        /// </summary>
        uint Size { get; }

        /// <summary>
        /// Gets or sets the write position of the current <see cref="IAudioBuffer"/>.
        /// </summary>
        uint WritePosition { get; set; }

        /// <summary>
        /// Gets or sets the read position of the current <see cref="IAudioBuffer"/>.
        /// </summary>
        uint ReadPosition { get; set; }
    }
}
