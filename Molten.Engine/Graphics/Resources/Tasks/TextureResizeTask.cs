
namespace Molten.Graphics;

public class TextureResizeTask : GraphicsResourceTask<GraphicsTexture, TextureResizeTask>
{
    public TextureDimensions NewDimensions;

    public GraphicsFormat NewFormat;

    public override void ClearForPool()
    {
        NewDimensions = new TextureDimensions();
        NewFormat = GraphicsFormat.Unknown;
    }

    public override void Validate()
    {
        throw new NotImplementedException();
    }

    protected override bool OnProcess(GraphicsQueue queue)
    {
        Resource.ResizeTexture(NewDimensions, NewFormat);
        return true;
    }
}
