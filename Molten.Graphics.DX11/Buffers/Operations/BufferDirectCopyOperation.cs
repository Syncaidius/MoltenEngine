using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal struct BufferDirectCopyOperation : IBufferOperation
    {
        internal GraphicsBuffer SrcBuffer;

        internal GraphicsBuffer DestBuffer;

        internal Action CompletionCallback;

        public unsafe void Process(GraphicsCommandQueue cmd)
        {
            // If the current buffer is a staging buffer, initialize and apply all its pending changes.
            if (SrcBuffer.Desc.Usage == Usage.Staging)
                SrcBuffer.Apply(cmd);

            (cmd as CommandQueueDX11).Native->CopyResource(DestBuffer, SrcBuffer);
            CompletionCallback?.Invoke();
        }
    }
}
