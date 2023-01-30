namespace Molten.Graphics
{
    internal struct BufferSetOperation<T> : IBufferOperation
        where T : unmanaged
    {
        /// <summary>The number of bytes to offset the change, from the start of the provided <see cref="Segment"/>.</summary>
        public uint ByteOffset;

        /// <summary>The number of bytes per element in <see cref="Data"/>.</summary>
        public uint DataStride;

        /// <summary>The number of elements to be copied.</summary>
        public uint Count;

        internal uint StartIndex;

        /// <summary>The data to be set.</summary>
        public T[] Data;

        public IGraphicsBufferSegment DestinationSegment;

        internal Action CompletionCallback;

        internal IStagingBuffer Staging;

        public void Process(GraphicsCommandQueue cmd)
        {
            GraphicsBuffer buffer = DestinationSegment.Buffer as GraphicsBuffer;
            buffer.Set(cmd as CommandQueueDX11, Data, StartIndex, Count, DataStride, ByteOffset, Staging as StagingBuffer);
            CompletionCallback?.Invoke();
        }
    }
}
