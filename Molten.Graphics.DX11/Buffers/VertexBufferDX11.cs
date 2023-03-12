using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public abstract unsafe class VertexBufferDX11 : GraphicsBuffer, IVertexBuffer
    {
        protected VertexBufferDX11(DeviceDX11 device, BufferMode mode, uint stride, uint numElements, void* initialData = null) :
            base(device, mode, BindFlag.VertexBuffer, stride, numElements, ResourceMiscFlag.None, StagingBufferFlags.None, initialData)
        {
            
        }

        /// <inheritdoc/>
        public VertexFormat VertexFormat { get; protected set; }
    }

    public unsafe class VertexBufferDX11<T> : VertexBufferDX11, IVertexBuffer
        where T : unmanaged, IVertexType
    {
        public unsafe VertexBufferDX11(DeviceDX11 device, BufferMode mode, uint numElements, void* initialData = null) : 
            base(device, mode, (uint)sizeof(T), numElements, initialData)
        {
            VertexFormat = device.VertexFormatCache.Get<T>();
        }
    }
}
