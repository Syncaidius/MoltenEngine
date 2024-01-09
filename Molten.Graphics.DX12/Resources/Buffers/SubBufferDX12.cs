namespace Molten.Graphics.DX12;
public class SubBufferDX12 : BufferDX12
{

    public SubBufferDX12(BufferDX12 parent, ulong offset, uint stride, ulong numElements, GraphicsResourceFlags flags, GraphicsBufferType type) : 
        base(parent.Device, stride, numElements, flags, type)
    {
        Offset = offset;
        ParentBuffer = parent;

        if(parent is SubBufferDX12 sub)
            RootBuffer = sub.RootBuffer ?? parent;
        else
            RootBuffer = parent;
    }

    protected override void OnCreateResource(uint frameBufferSize, uint frameBufferIndex, ulong frameID)
    {
        // TODO Only create sub-buffer views. Sub-buffers do not need their own resource, as this is provided by the root buffer.
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

    /// <summary>
    /// Gets the root <see cref="BufferDX12"/> instance. This is the top-most buffer, regardless of how many nested sub-buffers we allocated.
    /// </summary>
    internal BufferDX12 RootBuffer { get; private set; }

    internal bool IsFree { get; set; }
}
