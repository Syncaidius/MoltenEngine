using Silk.NET.DXGI;

namespace Molten.Graphics
{
    internal unsafe class IndexBufferBinder : ContextSlotBinder<BufferSegment>
    {
        internal override void Bind(ContextSlot<BufferSegment> slot, BufferSegment value)
        {
            slot.Cmd.Native->IASetIndexBuffer(value, value.DataFormat, value.ByteOffset);
        }

        internal override void Unbind(ContextSlot<BufferSegment> slot, BufferSegment value)
        {
            slot.Cmd.Native->IASetIndexBuffer(null, Format.FormatUnknown, 0);
        }
    }
}
