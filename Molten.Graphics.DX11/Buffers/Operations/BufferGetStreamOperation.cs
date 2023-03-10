using Molten.IO;

namespace Molten.Graphics
{
    internal struct BufferGetStreamOperation : IBufferOperation
    {
        internal BufferSegment Segment;

        internal IStagingBuffer Staging;

        /// <summary>A callback to interact with the retrieved stream.</summary>
        internal Action<IGraphicsBuffer, RawStream> StreamCallback;

        public void Process(GraphicsCommandQueue cmd)
        {
            Segment.GetStream(GraphicsPriority.Immediate, StreamCallback, Staging);
        }
    }
}
