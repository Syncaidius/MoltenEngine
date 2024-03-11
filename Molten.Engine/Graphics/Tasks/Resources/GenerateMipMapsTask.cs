namespace Molten.Graphics;

internal class GenerateMipMapsTask : GraphicsResourceTask<GpuTexture>
{
    public override void ClearForPool() { }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GpuCommandList cmd)
    {
        cmd.GenerateMipMaps(Resource);
        Resource.Version++;
        return true;
    }
}
