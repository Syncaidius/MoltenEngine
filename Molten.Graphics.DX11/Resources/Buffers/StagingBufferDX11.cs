using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class StagingBufferDX11 : BufferDX11
    {
        /// <summary>Creates a new instance of <see cref="StagingBufferDX11"/>.</summary>
        /// <param name="device">The graphics device to bind the buffer to.</param>
        /// <param name="numBytes">The number of elements the buffer should be able to hold.</param>
        /// <param name="flags">The flags to use when creating the buffer.</param>
        internal unsafe StagingBufferDX11(DeviceDX11 device, GraphicsResourceFlags flags, uint numBytes)
            : base(device, GraphicsBufferType.Staging, flags | GraphicsResourceFlags.GpuWrite | GraphicsResourceFlags.NoShaderAccess, BindFlag.None, 1, numBytes, ResourceMiscFlag.None)
        {
            
        }
    }
}
