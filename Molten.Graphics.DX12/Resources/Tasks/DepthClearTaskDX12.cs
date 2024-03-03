namespace Molten.Graphics.DX12;

internal class DepthClearTaskDX12 : GraphicsResourceTask<DepthSurfaceDX12>
{
    public float DepthValue;

    public byte StencilValue;

    public override void ClearForPool()
    {
        DepthValue = 1.0f;
        StencilValue = 0;
    }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GraphicsQueue queue)
    {
        Resource.Apply(queue);
        (queue as GraphicsQueueDX12).Clear(Resource, DepthValue, StencilValue);
        return true;
    }
}
