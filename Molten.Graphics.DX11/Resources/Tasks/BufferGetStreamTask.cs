using Molten.IO;

namespace Molten.Graphics
{
    internal struct BufferGetStreamTask : IGraphicsResourceTask
    {
        internal uint ByteOffset;

        internal uint Stride;

        internal uint NumElements;

        internal IStagingBuffer Staging;

        /// <summary>A callback to interact with the retrieved stream.</summary>
        internal Action<IGraphicsBuffer, GraphicsStream> StreamCallback;

        public bool Process(GraphicsCommandQueue cmd, GraphicsResource resource)
        {
            (resource as BufferDX11).GetStream(cmd as CommandQueueDX11, ByteOffset, Stride, NumElements, StreamCallback);
            return false;
        }
    }
}
