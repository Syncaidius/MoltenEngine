using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class StagingBuffer : BufferDX11, IStagingBuffer
    {
        /// <summary>Creates a new instance of <see cref="StagingBuffer"/>.</summary>
        /// <param name="device">The graphics device to bind the buffer to.</param>
        /// <param name="stagingFlags">Access flags for the buffer.</param>
        /// <param name="capacity">The number of elements the buffer should be able to hold.</param>
        internal unsafe StagingBuffer(DeviceDX11 device, BufferFlags flags, uint capacity)
            : base(device, flags, BindFlag.None, capacity, ResourceMiscFlag.None)
        {
            
        }
    }
}
