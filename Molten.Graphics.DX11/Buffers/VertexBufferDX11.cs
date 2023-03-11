using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public class VertexBufferDX11<T> : GraphicsBuffer, IVertexBuffer
        where T : unmanaged, IVertexType
    {
        public unsafe VertexBufferDX11(DeviceDX11 device, BufferMode mode, uint numElements, T[] initialData = null) : 
            base(device, mode, BindFlag.VertexBuffer, (uint)sizeof(T), numElements, ResourceMiscFlag.None, StagingBufferFlags.None, initialData)
        {
            VertexFormat = device.VertexFormatCache.Get<T>();
        }

        /// <inheritdoc/>
        public VertexFormat VertexFormat { get; }
    }
}
