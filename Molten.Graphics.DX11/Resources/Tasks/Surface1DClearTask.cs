namespace Molten.Graphics.DX11;

internal class Surface1DClearTask : GraphicsResourceTask<RenderSurface1DDX11>
{
    public Color Color;

    public override void ClearForPool()
    {
        Color = Color.Black;
    }

    public override void Validate()
    {
        throw new NotImplementedException();
    }

    protected override bool OnProcess(GraphicsQueue queue)
    {
        Resource.Ensure(queue);
        Resource.OnClear(queue as GraphicsQueueDX11, Color);
        return false;
    }
}
