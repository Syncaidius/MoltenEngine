namespace Molten.Graphics;

public class TextureResizeTask : GraphicsResourceTask<GraphicsTexture>
{
    public TextureDimensions NewDimensions;

    public GraphicsFormat NewFormat;

    public override void ClearForPool()
    {
        NewDimensions = new TextureDimensions();
        NewFormat = GraphicsFormat.Unknown;
    }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GraphicsQueue queue)
    {
        Resource.ResizeTexture(NewDimensions, NewFormat);
        Resource.Version++;
        return true;
    }
}
