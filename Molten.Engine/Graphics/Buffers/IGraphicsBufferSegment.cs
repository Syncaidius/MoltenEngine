namespace Molten.Graphics
{
    public interface IGraphicsBufferSegment : IShaderResource
    {
        void SetData<T>(T[] data)
            where T : unmanaged;

        void SetData<T>(T[] data, uint count)
            where T : unmanaged;

        void SetData<T>(T[] data, uint startIndex, uint count, uint elementOffset = 0, IStagingBuffer staging = null, Action completionCallback = null)
            where T : unmanaged;

        void Release();

        void SetVertexFormat<T>() where T : struct, IVertexType;

        void SetIndexFormat(IndexBufferFormat format);

        /// <summary>
        /// Gets the parent <see cref="IGraphicsBuffer"/> of the current <see cref="IGraphicsBufferSegment"/>.
        /// </summary>
        IGraphicsBuffer Buffer { get; }

        /// <summary>
        /// Gets the byte offset within the parent <see cref="Buffer"/>.
        /// </summary>
        public uint ByteOffset { get; }

        /// <summary>Gets the size of the segment in bytes. This is <see cref="ElementCount"/> multiplied by <see cref="Stride"/>.</summary>
        uint ByteCount { get; }

        /// <summary>Gets the number of elements that the segment can hold.</summary>
        uint ElementCount { get; }

        /// <summary>
        /// Gets the size of each element within the buffer, in bytes.
        /// </summary>
        uint Stride { get; }

        VertexFormat VertexFormat { get; }
    }
}
