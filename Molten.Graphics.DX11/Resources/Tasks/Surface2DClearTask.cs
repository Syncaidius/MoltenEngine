namespace Molten.Graphics.DX11;

internal class Surface2DClearTask : GraphicsResourceTask<RenderSurface2DDX11>
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

    protected override bool OnProcess(RenderService renderer, GraphicsQueue queue)
    {
        Resource.Apply(queue);
        Resource.OnClear(queue as GraphicsQueueDX11, Color);
        return true;
    }
}
