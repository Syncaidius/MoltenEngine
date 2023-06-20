namespace Molten.Graphics.DX11
{
    internal struct DepthClearTask : IGraphicsResourceTask
    {
        public DepthSurfaceDX11 Surface;

        public DepthClearFlags Flags;

        public float DepthClearValue;

        public byte StencilClearValue;

        public bool Process(GraphicsQueue cmd, GraphicsResource resource)
        {
            Surface.EnsureResource(cmd);
            Surface.OnClear(cmd as GraphicsQueueDX11, ref this);
            return false;
        }
    }
}
