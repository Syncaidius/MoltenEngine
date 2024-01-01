namespace Molten.Graphics
{
    internal struct GenerateMipMapsTask : IGraphicsResourceTask
    {
        internal Action<GraphicsResource> OnCompleted;

        public unsafe bool Process(GraphicsQueue cmd, GraphicsResource resource)
        {
            cmd.GenerateMipMaps(resource);
            OnCompleted?.Invoke(resource);

            return true;
        }
    }
}
