namespace Molten.Graphics;

internal class GenerateMipMapsTask : GraphicsResourceTask<GraphicsTexture>
{
    internal Action<GraphicsTexture> OnCompleted;

    public override void ClearForPool()
    {
        OnCompleted = null;
    }

    public override void Validate()
    {
        throw new NotImplementedException();
    }

    protected override bool OnProcess(GraphicsQueue queue)
    {
        queue.GenerateMipMaps(Resource);
        OnCompleted?.Invoke(Resource);

        return true;
    }
}
