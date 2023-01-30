namespace Molten.Graphics
{
    public interface IGraphicsBuffer : IShaderResource
    {
        void Defragment();

        IGraphicsBufferSegment Allocate<T>(uint count) where T : unmanaged;

        IGraphicsBufferSegment UpdateAllocation<T>(IGraphicsBufferSegment existing, uint count) where T : unmanaged;

        void Deallocate(IGraphicsBufferSegment segment);

        uint StructuredStride { get; }

        uint ByteCapacity { get; }

        BufferMode Mode { get; }
    }
}
