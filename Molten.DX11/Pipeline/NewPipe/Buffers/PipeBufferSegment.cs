using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class PipeBufferSegment : PipeBindableResource<ID3D11Buffer>
    {
        internal PipeBuffer Buffer { get; }

        /// <summary>The size of the segment in bytes. This is <see cref="ElementCount"/> multiplied by <see cref="Stride"/>.</summary>
        internal uint ByteCount;

        /// <summary>The number of elements that the segment can hold.</summary>
        internal uint ElementCount;

        /// <summary>The previous segment (if any) within the mapped buffer.</summary>
        internal BufferSegment Previous;

        /// <summary>The next segment (if any) within the mapped buffer.</summary>
        internal BufferSegment Next;

        /// <summary>
        /// The byte offset within the <see cref="Buffer"/> <see cref="GraphicsBuffer"/>.
        /// </summary>
        internal uint ByteOffset;

        /// <summary>
        /// The size of each element within the buffer, in bytes.
        /// </summary>
        internal uint Stride;

        internal VertexFormat VertexFormat;

        internal Format DataFormat;

        /// <summary>If true, the segment is not used.</summary>
        internal bool IsFree;

        internal PipeBufferSegment(PipeBuffer parent) : 
            base(parent.CanBindTo, parent.BindTypeFlags)
        {
            Buffer = parent;
        }

        internal override unsafe ID3D11Buffer* Native => Buffer.Native;

        protected override void OnBind(PipeBindSlot slot, PipeDX11 pipe)
        {
            throw new NotImplementedException();
        }

        protected override void OnDispose()
        {
            throw new NotImplementedException();
        }
    }
}
