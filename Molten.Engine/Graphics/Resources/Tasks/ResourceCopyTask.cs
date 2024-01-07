namespace Molten.Graphics;

public class ResourceCopyTask : GraphicsResourceTask<GraphicsResource>
{
    public GraphicsResource Destination;

    public Action<GraphicsResource> CompletionCallback;

    public override void ClearForPool()
    {
        Destination = null;
        CompletionCallback = null;
    }

    public override void Validate()
    {
        throw new NotImplementedException();
    }

    protected override bool OnProcess(GraphicsQueue queue)
    {
        if (Resource is GraphicsBuffer buffer && buffer.BufferType == GraphicsBufferType.Staging)
            Resource.Ensure(queue);

        queue.CopyResource(Resource, Destination);
        CompletionCallback?.Invoke(Resource);

        return false;
    }
}
