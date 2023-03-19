namespace Molten.Graphics
{
    internal struct GenerateMipMapsTask : IGraphicsResourceTask
    {
        public unsafe bool Process(GraphicsCommandQueue cmd, GraphicsResource resource)
        {
            Texture2DDX11 tex = resource as Texture2DDX11;
            if (tex.SRV.Ptr != null)
                (cmd as CommandQueueDX11).Native->GenerateMips(tex.SRV);

            return true;
        }
    }
}
