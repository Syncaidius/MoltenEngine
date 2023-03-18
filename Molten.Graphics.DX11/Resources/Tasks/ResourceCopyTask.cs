using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal struct ResourceCopyTask : IGraphicsResourceTask
    {
        internal ResourceDX11 Destination;

        internal Action<GraphicsResource> CompletionCallback;

        public unsafe bool Process(GraphicsCommandQueue cmd, GraphicsResource resource)
        {
            ResourceDX11 src = resource as ResourceDX11;

            // If the current buffer is a staging buffer, initialize and apply all its pending changes.
            if (src.UsageFlags == Usage.Staging)
                src.Apply(cmd);

            (cmd as CommandQueueDX11).Native->CopyResource(Destination, src);
            CompletionCallback?.Invoke(resource);

            return false;
        }
    }
}
