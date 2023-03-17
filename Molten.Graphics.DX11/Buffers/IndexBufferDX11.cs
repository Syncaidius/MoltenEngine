using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public class IndexBufferDX11 : BufferDX11, IIndexBuffer
    {
        public unsafe IndexBufferDX11(DeviceDX11 device, BufferFlags mode, IndexBufferFormat format, uint numElements, void* initialData) :
            base(device, mode, BindFlag.IndexBuffer, (uint)format, numElements, ResourceMiscFlag.None, initialData)
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
