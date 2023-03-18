using Molten.IO;

namespace Molten.Graphics
{
    internal struct BufferGetStreamOperation : IGraphicsResourceTask
    {
        internal uint ByteOffset;

        internal uint Stride;

        internal uint NumElements;

        internal BufferDX11 SrcBuffer;

        internal IStagingBuffer Staging;

        /// <summary>A callback to interact with the retrieved stream.</summary>
        internal Action<IGraphicsBuffer, RawStream> StreamCallback;

        public void Process(GraphicsCommandQueue cmd)
        {
            SrcBuffer.GetStream(cmd as CommandQueueDX11, ByteOffset, Stride, NumElements, StreamCallback);
        }
    }
}
