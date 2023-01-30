using Silk.NET.DXGI;

namespace Molten.Graphics
{
    internal unsafe class IndexBufferBinder : GraphicsSlotBinder<IGraphicsBufferSegment>
    {
        public override void Bind(GraphicsSlot<IGraphicsBufferSegment> slot, IGraphicsBufferSegment value)
        {
            BufferSegment buffer = value as BufferSegment;
            (slot.Cmd as CommandQueueDX11).Native->IASetIndexBuffer(buffer, buffer.DataFormat, value.ByteOffset);
        }

        public override void Unbind(GraphicsSlot<IGraphicsBufferSegment> slot, IGraphicsBufferSegment value)
        {
            (slot.Cmd as CommandQueueDX11).Native->IASetIndexBuffer(null, Format.FormatUnknown, 0);
        }
    }
}
