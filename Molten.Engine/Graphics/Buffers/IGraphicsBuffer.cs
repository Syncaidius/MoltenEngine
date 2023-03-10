using Molten.Graphics;

namespace Molten.Graphics
{
    public interface IGraphicsBuffer : IShaderResource
    {
        void Defragment();

        IGraphicsBufferSegment Allocate<T>(uint count) where T : unmanaged;

        IGraphicsBufferSegment UpdateAllocation<T>(IGraphicsBufferSegment existing, uint count) where T : unmanaged;

        void GetData<T>(GraphicsPriority priority, T[] destination, uint startIndex, uint count, uint elementOffset, Action<T[]> completionCallback = null)
            where T : unmanaged;

        void CopyTo(GraphicsPriority priority, IGraphicsBuffer destination, Action completionCallback = null);

        void CopyTo(GraphicsPriority priority, IGraphicsBuffer destination, ResourceRegion sourceRegion, uint destByteOffset = 0, Action completionCallback = null);

        void Deallocate(IGraphicsBufferSegment segment);

        uint StructuredStride { get; }

        uint ByteCapacity { get; }

        BufferMode Mode { get; }
    }
}
