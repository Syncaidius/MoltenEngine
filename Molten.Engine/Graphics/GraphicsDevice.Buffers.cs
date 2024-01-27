namespace Molten.Graphics;

public abstract partial class GraphicsDevice
{
    public GraphicsBuffer CreateVertexBuffer<T>(GraphicsResourceFlags flags, uint vertexCapacity, T[] initialData = null)
        where T : unmanaged, IVertexType
    {
        flags |= GraphicsResourceFlags.DenyShaderAccess;
        GraphicsBuffer buffer = CreateBuffer(GraphicsBufferType.Vertex, flags, GraphicsFormat.Unknown, vertexCapacity, initialData);
        buffer.VertexLayout = LayoutCache.GetVertexLayout<T>();

        return buffer;
    }

    public GraphicsBuffer CreateIndexBuffer(ushort[] data, GraphicsResourceFlags flags = GraphicsResourceFlags.None)
    {
        return CreateIndexBuffer(flags, (uint)data.Length, data);
    }

    public GraphicsBuffer CreateIndexBuffer(uint[] data, GraphicsResourceFlags flags = GraphicsResourceFlags.None)
    {
        return CreateIndexBuffer(flags, (uint)data.Length, data);
    }

    public GraphicsBuffer CreateIndexBuffer(byte[] data, GraphicsResourceFlags flags = GraphicsResourceFlags.None)
    {
        return CreateIndexBuffer(flags, (uint)data.Length, data);
    }

    public GraphicsBuffer CreateIndexBuffer(GraphicsResourceFlags flags, uint indexCapacity, ushort[] initialData)
    {
        return CreateBuffer(GraphicsBufferType.Index, flags, GraphicsFormat.R16_UInt, indexCapacity, initialData);
    }

    public GraphicsBuffer CreateIndexBuffer(GraphicsResourceFlags flags, uint indexCapacity, uint[] initialData = null)
    {
        flags |= GraphicsResourceFlags.DenyShaderAccess;
        return CreateBuffer(GraphicsBufferType.Index, flags, GraphicsFormat.R32_UInt, indexCapacity, initialData);
    }

    public GraphicsBuffer CreateIndexBuffer(GraphicsResourceFlags flags, uint indexCapacity, byte[] initialData = null)
    {
        flags |= GraphicsResourceFlags.DenyShaderAccess;
        return CreateBuffer(GraphicsBufferType.Index, flags, GraphicsFormat.R8_UInt, indexCapacity, initialData);
    }

    public GraphicsBuffer CreateStructuredBuffer<T>(T[] data, GraphicsResourceFlags flags = GraphicsResourceFlags.None)
        where T : unmanaged
    {
        return CreateStructuredBuffer(flags, (uint)data.Length, data);
    }

    public GraphicsBuffer CreateStructuredBuffer<T>(GraphicsResourceFlags flags, uint elementCapacity, T[] initialData = null)
        where T : unmanaged
    {
        return CreateBuffer(GraphicsBufferType.Structured, flags, GraphicsFormat.Unknown, elementCapacity, initialData);
    }

    public GraphicsBuffer CreateStagingBuffer(bool allowCpuRead, bool allowCpuWrite, uint byteCapacity)
    {
        GraphicsResourceFlags flags = GraphicsResourceFlags.GpuWrite | GraphicsResourceFlags.DenyShaderAccess;

        if (allowCpuRead)
            flags |= GraphicsResourceFlags.CpuRead;

        if (allowCpuWrite)
            flags |= GraphicsResourceFlags.CpuWrite;

        return CreateBuffer<byte>(GraphicsBufferType.Staging, flags, GraphicsFormat.Unknown, byteCapacity, null);
    }

    protected abstract GraphicsBuffer CreateBuffer<T>(GraphicsBufferType type, GraphicsResourceFlags flags, GraphicsFormat format,
        uint numElements, T[] initialData) where T : unmanaged;
}
