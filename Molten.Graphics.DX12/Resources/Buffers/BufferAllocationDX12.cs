namespace Molten.Graphics.DX12;
public class BufferAllocationDX12 : BufferDX12
{
    public ulong Offset;
    public bool IsFree;

    public BufferAllocationDX12(BufferDX12 parent, ulong offset, uint stride, ulong numElements, GraphicsResourceFlags flags, GraphicsBufferType type) : 
        base(parent.Device, stride, numElements, flags, type)
    {
        Offset = offset;
        ParentBuffer = parent;
    }

    protected override void OnCreateResource(uint frameBufferSize, uint frameBufferIndex, ulong frameID)
    {
        throw new NotImplementedException();
    }

    protected override void OnFrameBufferResized(uint lastFrameBufferSize, uint frameBufferSize, uint frameBufferIndex, ulong frameID)
    {
        throw new NotImplementedException();
    }

    protected override void OnGraphicsRelease()
    {
        throw new NotImplementedException();
    }

    protected override void OnNextFrame(GraphicsQueue queue, uint frameBufferIndex, ulong frameID)
    {
        throw new NotImplementedException();
    }

    public override ResourceHandleDX12 Handle => throw new NotImplementedException();

    public override GraphicsFormat ResourceFormat { get; protected set; }

    internal BufferDX12 ParentBuffer { get; }
}
