using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal struct BufferDirectCopyOperation : IBufferOperation
    {
        internal GraphicsBuffer SourceBuffer;

        internal GraphicsBuffer DestinationBuffer;

        internal Action CompletionCallback;

        public void Process(PipeDX11 pipe)
        {
            SourceBuffer.CopyTo(pipe, DestinationBuffer);
            CompletionCallback?.Invoke();
        }
    }
}
