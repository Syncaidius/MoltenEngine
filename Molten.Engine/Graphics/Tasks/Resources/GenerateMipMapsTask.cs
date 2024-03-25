namespace Molten.Graphics;

internal class GenerateMipMapsTask : GpuResourceTask<GpuTexture>
{
    public override void ClearForPool() { }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GpuCommandList cmd)
    {
        cmd.OnGenerateMipmaps(Resource);
        Resource.Version++;
        return true;
    }
}
