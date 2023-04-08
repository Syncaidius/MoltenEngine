using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal struct GenerateMipMapsTask : IGraphicsResourceTask
    {
        public unsafe bool Process(GraphicsQueue cmd, GraphicsResource resource)
        {
            if (resource.SRV != null)
                (cmd as CommandQueueDX11).Native->GenerateMips((ID3D11ShaderResourceView*)resource.SRV);

            return true;
        }
    }
}
