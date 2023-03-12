using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public class IndexBufferDX11 : GraphicsBuffer, IIndexBuffer
    {
        public unsafe IndexBufferDX11(DeviceDX11 device, BufferMode mode, IndexBufferFormat format, uint numElements, void* initialData) : 
            base(device, 
                mode, 
                BindFlag.IndexBuffer, 
                format == IndexBufferFormat.UInt32 ? 4U : 2U, 
                numElements,
                ResourceMiscFlag.None, 
                StagingBufferFlags.None,
                initialData)
        {
            IndexFormat = format;

            switch (format)
            {
                case IndexBufferFormat.UInt32:
                    D3DFormat = Format.FormatR32Uint;
                    break;

                case IndexBufferFormat.UInt16:
                    D3DFormat = Format.FormatR16Uint;
                    break;
            }
        }

        /// <inheritdoc/>
        public IndexBufferFormat IndexFormat { get; }

        internal Format D3DFormat { get; }
    }
}
