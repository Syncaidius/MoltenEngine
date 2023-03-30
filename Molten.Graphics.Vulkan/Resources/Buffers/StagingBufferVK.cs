using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal class StagingBufferVK : BufferVK
    {
        internal unsafe StagingBufferVK(GraphicsDevice device, uint stride, uint numElements) : 
            base(device, GraphicsBufferType.Staging, 
                GraphicsResourceFlags.CpuRead | GraphicsResourceFlags.CpuWrite | GraphicsResourceFlags.GpuWrite, 
                BufferUsageFlags.None, stride, numElements, null)
        {
        }
    }
}
