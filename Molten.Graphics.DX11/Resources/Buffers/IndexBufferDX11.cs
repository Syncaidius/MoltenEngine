using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public class IndexBufferDX11 : BufferDX11
    {
        public unsafe IndexBufferDX11(DeviceDX11 device, GraphicsResourceFlags mode, IndexBufferFormat format, uint numElements, void* initialData) :
            base(device, GraphicsBufferType.IndexBuffer, mode, BindFlag.IndexBuffer, (uint)format, numElements, ResourceMiscFlag.None, initialData)
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

        internal Format D3DFormat { get; }
    }
}
