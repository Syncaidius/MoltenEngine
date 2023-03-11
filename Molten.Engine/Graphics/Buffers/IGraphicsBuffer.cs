using Molten.Graphics;

namespace Molten.Graphics
{
    public interface IGraphicsBuffer : IShaderResource
    {
        void SetData<T>(GraphicsPriority priority, T[] data, uint startIndex, uint elementCount, uint byteOffset = 0, IStagingBuffer staging = null, Action completeCallback = null) 
            where T : unmanaged;

        void GetData<T>(GraphicsPriority priority, T[] destination, uint startIndex, uint count, uint elementOffset, Action<T[]> completionCallback = null)
            where T : unmanaged;

        void CopyTo(GraphicsPriority priority, IGraphicsBuffer destination, Action completionCallback = null);

        void CopyTo(GraphicsPriority priority, IGraphicsBuffer destination, ResourceRegion sourceRegion, uint destByteOffset = 0, Action completionCallback = null);
        
        /// <summary>
        /// Gets the stride (byte size) of each element within the current <see cref="IGraphicsBuffer"/>.
        /// </summary>
        uint Stride { get; }

        /// <summary>
        /// Gets the capacity of a single section within the buffer, in bytes.
        /// </summary>
        uint ByteCapacity { get; }

        /// <summary>
        /// Gets the number of elements that the current <see cref="IGraphicsBuffer"/> can store.
        /// </summary>
        uint ElementCount { get; }

        /// <summary>Gets the flags that were passed in to the buffer when it was created.</summary>
        BufferMode Mode { get; }
    }
}
