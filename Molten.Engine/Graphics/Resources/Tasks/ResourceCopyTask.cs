namespace Molten.Graphics;

public class ResourceCopyTask : GraphicsResourceTask<GraphicsResource>
{
    public GraphicsResource Destination;

    public override void ClearForPool()
    {
        Destination = null;
    }

    public override void Validate()
    {
        throw new NotImplementedException();
    }

    protected override bool OnProcess(RenderService renderer, GraphicsQueue queue)
    {
        if (Resource is GraphicsBuffer buffer && buffer.BufferType == GraphicsBufferType.Staging)
            Resource.Ensure(queue);

        queue.CopyResource(Resource, Destination);

        return true;
    }
}
