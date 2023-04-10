using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11
{
    internal struct GenerateMipMapsTask : IGraphicsResourceTask
    {
        public unsafe bool Process(GraphicsQueue cmd, GraphicsResource resource)
        {
            if (resource.SRV != null)
                (cmd as GraphicsQueueDX11).Ptr->GenerateMips((ID3D11ShaderResourceView*)resource.SRV);

            return true;
        }
    }
}
