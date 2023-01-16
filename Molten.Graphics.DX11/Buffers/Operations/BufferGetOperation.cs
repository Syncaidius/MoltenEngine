namespace Molten.Graphics
{
    internal struct BufferGetOperation<T> : IBufferOperation where T : unmanaged
    {
        /// <summary>The number of bytes to offset the change, from the start of the provided <see cref="SourceSegment"/>.</summary>
        internal uint ByteOffset;

        /// <summary>The number of bytes per element in <see cref="Data"/>.</summary>
        internal uint DataStride;

        /// <summary>The number of elements to be copied.</summary>
        internal uint Count;

        /// <summary>The first index at which to start placing the retrieved data within <see cref="DestinationArray"/>.</summary>
        internal uint DestinationIndex;

        internal BufferSegment SourceSegment;

        /// <summary>A callback to send the retrieved data to.</summary>
        internal Action<T[]> CompletionCallback;

        /// <summary>The destination array to store the retrieved data.</summary>
        internal T[] DestinationArray;

        public void Process(CommandQueueDX11 context)
        {
            DestinationArray = DestinationArray ?? new T[Count];
            SourceSegment.Buffer.Get<T>(context, DestinationArray, 0, ByteOffset, DataStride, Count);

            CompletionCallback.Invoke(DestinationArray);
        }
    }
}
