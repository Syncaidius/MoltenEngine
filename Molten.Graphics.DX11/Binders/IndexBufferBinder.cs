using Silk.NET.DXGI;

namespace Molten.Graphics
{
    internal unsafe class IndexBufferBinder : GraphicsSlotBinder<BufferSegment>
    {
        public override void Bind(GraphicsSlot<BufferSegment> slot, BufferSegment value)
        {
            (slot.Cmd as CommandQueueDX11).Native->IASetIndexBuffer(value, value.DataFormat, value.ByteOffset);
        }

        public override void Unbind(GraphicsSlot<BufferSegment> slot, BufferSegment value)
        {
            (slot.Cmd as CommandQueueDX11).Native->IASetIndexBuffer(null, Format.FormatUnknown, 0);
        }
    }
}
