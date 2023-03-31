using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public class IndexBufferDX11 : BufferDX11
    {
        public unsafe IndexBufferDX11(DeviceDX11 device, GraphicsResourceFlags mode, IndexBufferFormat format, uint numElements, void* initialData, uint initialBytes) :
            base(device, GraphicsBufferType.Index, mode, (uint)format, numElements, initialData, initialBytes)
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


            ResourceFormat = D3DFormat.FromApi();
        }

        internal Format D3DFormat { get; }

        public override GraphicsFormat ResourceFormat { get; protected set; }
    }
}
