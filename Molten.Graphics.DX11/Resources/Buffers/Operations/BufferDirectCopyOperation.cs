using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal struct BufferDirectCopyOperation : IGraphicsResourceTask
    {
        internal BufferDX11 DestBuffer;

        internal Action CompletionCallback;

        public unsafe void Process(GraphicsCommandQueue cmd, GraphicsResource resource)
        {
            BufferDX11 srcBuffer = resource as BufferDX11;

            // If the current buffer is a staging buffer, initialize and apply all its pending changes.
            if (srcBuffer.Desc.Usage == Usage.Staging)
                srcBuffer.Apply(cmd);

            (cmd as CommandQueueDX11).Native->CopyResource(DestBuffer, srcBuffer);
            CompletionCallback?.Invoke();
        }
    }
}
