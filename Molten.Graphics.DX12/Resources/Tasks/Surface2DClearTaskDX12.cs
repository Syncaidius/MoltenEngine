namespace Molten.Graphics.DX12;

internal class Surface2DClearTaskDX12 : GraphicsResourceTask<RenderSurface2DDX12>
{
    public Color Color;

    public override void ClearForPool()
    {
        Color = Color.Black;
    }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GpuCommandQueue queue)
    {
        Resource.Apply(queue);
        (queue as GraphicsQueueDX12).ClearDSV(Resource, Color);
        return true;
    }
}
