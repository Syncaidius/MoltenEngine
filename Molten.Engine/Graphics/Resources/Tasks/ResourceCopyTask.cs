namespace Molten.Graphics
{
    public struct ResourceCopyTask : IGraphicsResourceTask
    {
        public GraphicsResource Destination;

        public Action<GraphicsResource> CompletionCallback;

        public unsafe bool Process(GraphicsCommandQueue cmd, GraphicsResource resource)
        {
            // If the current buffer is a staging buffer, initialize and apply all its pending changes.
            if (resource is GraphicsBuffer buffer && buffer.BufferType == GraphicsBufferType.StagingBuffer)
                resource.Apply(cmd);

            cmd.CopyResource(resource, Destination);
            CompletionCallback?.Invoke(resource);

            return false;
        }
    }
}
