namespace Molten.Graphics.DX11;

internal class DepthClearTask : GraphicsResourceTask<DepthSurfaceDX11>
{
    public DepthClearFlags Flags;

    public float DepthClearValue;

    public byte StencilClearValue;

    public override void ClearForPool()
    {
        Flags = DepthClearFlags.None;
        DepthClearValue = 1.0f;
        StencilClearValue = 0;
    }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GraphicsQueue queue)
    {
        Resource.Apply(queue);
        Resource.OnClear(queue as GraphicsQueueDX11, this);
        return true;
    }
}
