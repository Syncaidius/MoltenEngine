namespace Molten.Graphics;

public struct ResourceCopyTask : IGraphicsResourceTask
{
    public GraphicsResource Destination;

    public Action<GraphicsResource> CompletionCallback;

    public unsafe bool Process(GraphicsQueue cmd, GraphicsResource resource)
    {
        if (resource is GraphicsBuffer buffer && buffer.BufferType == GraphicsBufferType.Staging)
            resource.Ensure(cmd);

        cmd.CopyResource(resource, Destination);
        CompletionCallback?.Invoke(resource);

        return false;
    }
}
