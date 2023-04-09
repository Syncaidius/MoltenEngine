using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    internal unsafe class IndexBufferBinder : GraphicsSlotBinder<GraphicsBuffer>
    {
        public override void Bind(GraphicsSlot<GraphicsBuffer> slot, GraphicsBuffer value)
        {
            IndexBufferDX11 buffer = value as IndexBufferDX11;
            uint byteOffset = 0; // value.ByteOffset - May need again later for multi-part meshes.
            (slot.Cmd as GraphicsQueueDX11).Ptr->IASetIndexBuffer((ID3D11Buffer*)buffer.Handle, buffer.D3DFormat, byteOffset);
        }

        public override void Unbind(GraphicsSlot<GraphicsBuffer> slot, GraphicsBuffer value)
        {
            (slot.Cmd as GraphicsQueueDX11).Ptr->IASetIndexBuffer(null, Format.FormatUnknown, 0);
        }
    }
}
