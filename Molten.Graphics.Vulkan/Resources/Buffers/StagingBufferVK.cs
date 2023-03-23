using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal class StagingBufferVK : BufferVK, IStagingBuffer
    {
        internal unsafe StagingBufferVK(GraphicsDevice device, uint stride, uint numElements) : 
            base(device, GraphicsResourceFlags.CpuRead | GraphicsResourceFlags.CpuWrite | GraphicsResourceFlags.GpuWrite, BufferUsageFlags.None, stride, numElements, null)
        {
        }
    }
}
