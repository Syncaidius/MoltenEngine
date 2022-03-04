using Silk.NET.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class IndexBufferBinder : ContextSlotBinder<BufferSegment>
    {
        internal override void Bind(ContextSlot<BufferSegment> slot, BufferSegment value)
        {
            slot.Context.Native->IASetIndexBuffer(value, value.DataFormat, value.ByteOffset);
        }

        internal override void Unbind(ContextSlot<BufferSegment> slot, BufferSegment value)
        {
            slot.Context.Native->IASetIndexBuffer(null, Format.FormatUnknown, 0);
        }
    }
}
