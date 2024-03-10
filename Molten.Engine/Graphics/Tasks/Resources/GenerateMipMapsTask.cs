namespace Molten.Graphics;

internal class GenerateMipMapsTask : GraphicsResourceTask<GraphicsTexture>
{
    public override void ClearForPool() { }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GpuCommandQueue queue)
    {
        queue.GenerateMipMaps(Resource);
        Resource.Version++;
        return true;
    }
}
