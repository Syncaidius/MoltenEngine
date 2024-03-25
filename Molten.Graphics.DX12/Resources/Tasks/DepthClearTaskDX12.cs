namespace Molten.Graphics.DX12;

internal class DepthClearTaskDX12 : GpuResourceTask<DepthSurfaceDX12>
{
    public float DepthValue;

    public byte StencilValue;

    public DepthClearFlags ClearFlags;

    public override void ClearForPool()
    {
        DepthValue = 1.0f;
        StencilValue = 0;
        ClearFlags = DepthClearFlags.None;
    }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GpuCommandQueue queue)
    {
        Resource.Apply(queue);
        (queue as CommandQueueDX12).Clear(Resource, DepthValue, StencilValue, ClearFlags);
        return true;
    }
}
