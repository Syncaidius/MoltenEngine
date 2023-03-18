namespace Molten.Graphics
{
    internal struct DepthClearTask : IGraphicsResourceTask
    {
        public DepthSurfaceDX11 Surface;

        public DepthClearFlags Flags;

        public float DepthClearValue;

        public byte StencilClearValue;

        public bool Process(GraphicsCommandQueue cmd, GraphicsResource resource)
        {
            Surface.OnClear(cmd as CommandQueueDX11, ref this);
            return false;
        }
    }
}
