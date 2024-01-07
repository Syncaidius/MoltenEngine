namespace Molten.Graphics;

internal class GenerateMipMapsTask : GraphicsResourceTask<GraphicsTexture>
{
    public override void ClearForPool() { }

    public override void Validate()
    {
        throw new NotImplementedException();
    }

    protected override bool OnProcess(RenderService renderer, GraphicsQueue queue)
    {
        queue.GenerateMipMaps(Resource);
        Resource.Version++;
        return true;
    }
}
