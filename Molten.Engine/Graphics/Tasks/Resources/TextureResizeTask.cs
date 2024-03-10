namespace Molten.Graphics;

public class TextureResizeTask : GraphicsResourceTask<GraphicsTexture>
{
    public TextureDimensions NewDimensions;

    public GpuResourceFormat NewFormat;

    public override void ClearForPool()
    {
        NewDimensions = new TextureDimensions();
        NewFormat = GpuResourceFormat.Unknown;
    }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GpuCommandQueue queue)
    {
        Resource.ResizeTexture(NewDimensions, NewFormat);
        Resource.Version++;
        return true;
    }
}
