using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public class IndexBufferDX11 : GraphicsBuffer, IIndexBuffer
    {
        public unsafe IndexBufferDX11(DeviceDX11 device, BufferMode mode, IndexBufferFormat format, uint numElements, Array initialData = null) : 
            base(device, 
                mode, 
                BindFlag.VertexBuffer, 
                format == IndexBufferFormat.Unsigned32Bit ? 4U : 2U, 
                numElements,
                ResourceMiscFlag.None, 
                StagingBufferFlags.None,
                initialData)
        {
            IndexFormat = format;
        }

        /// <inheritdoc/>
        public IndexBufferFormat IndexFormat { get; }
    }
}
