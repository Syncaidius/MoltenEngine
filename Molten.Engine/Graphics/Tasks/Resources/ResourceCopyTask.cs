namespace Molten.Graphics;

public class ResourceCopyTask : GraphicsResourceTask<GraphicsResource>
{
    public GraphicsResource Destination;

    public override void ClearForPool()
    {
        Destination = null;
    }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GraphicsQueue queue)
    {
        if (Resource is GraphicsBuffer buffer && buffer.BufferType == GraphicsBufferType.Staging)
            Resource.Apply(queue);

        queue.CopyResource(Resource, Destination);

        return true;
    }
}
