namespace Molten.Graphics
{
    internal struct GenerateMipMapsTask : IGraphicsResourceTask
    {
        internal Action<GraphicsResource> OnCompleted;

        public unsafe bool Process(GraphicsQueue cmd, GraphicsResource resource)
        {
            (resource as GraphicsTexture).OnGenerateMipMaps(cmd);
            OnCompleted?.Invoke(resource);

            return true;
        }
    }
}
