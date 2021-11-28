using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal struct BufferCopyOperation : IBufferOperation
    {
        internal GraphicsBuffer SourceBuffer;

        internal GraphicsBuffer DestinationBuffer;

        internal ResourceRegion SourceRegion;

        /// <summary>The number of bytes to offset the data in the <see cref="DestinationBuffer"/>.</summary>
        internal uint DestinationByteOffset;

        /// <summary>The number of bytes per element in <see cref="Data"/>.</summary>
        public uint DataStride;

        internal Action CompletionCallback;

        public void Process(PipeDX11 pipe)
        {
            CompletionCallback?.Invoke();
        }
    }
}
