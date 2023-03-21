using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class StagingBuffer : BufferDX11, IStagingBuffer
    {
        /// <summary>Creates a new instance of <see cref="StagingBuffer"/>.</summary>
        /// <param name="device">The graphics device to bind the buffer to.</param>
        /// <param name="numBytes">The number of elements the buffer should be able to hold.</param>
        /// <param name="flags">The flags to use when creating the buffer.</param>
        internal unsafe StagingBuffer(DeviceDX11 device, GraphicsResourceFlags flags, uint numBytes)
            : base(device, flags, BindFlag.None, 1, numBytes, ResourceMiscFlag.None)
        {
            
        }
    }
}
