using System.Drawing;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal struct SubResourceCopyTask : IGraphicsResourceTask
    {
        internal Box? SrcRegion;

        internal uint SrcSubResource;

        /// <summary>The start offset within the resource.
        /// <para>For a buffer, only the X dimension needs to be set equal to the number of bytes to offset.</para>
        /// <para>For textures, this will vary depending on the number of texture dimensions.</para></summary>
        internal Vector3UI DestStart;

        internal ResourceDX11 DestResource;

        internal uint DestSubResource;

        internal Action<GraphicsResource> CompletionCallback;

        public unsafe bool Process(GraphicsCommandQueue cmd, GraphicsResource resource)
        {
            ResourceDX11 src = resource as ResourceDX11;
            ValidateUsage(src);

            // If the current buffer is a staging buffer, initialize and apply all its pending changes.
            if (src.UsageFlags == Usage.Staging)
                src.Apply(cmd);

            CommandQueueDX11 dxCmd = cmd as CommandQueueDX11;
            if (SrcRegion.HasValue)
            {
                Box region = SrcRegion.Value;
                dxCmd.CopyResourceRegion(src, SrcSubResource, &region, DestResource, DestSubResource, DestStart);
            }
            else
            {
                dxCmd.CopyResourceRegion(src, SrcSubResource, null, DestResource, DestSubResource, DestStart);
            }

            cmd.Profiler.Current.CopySubresourceCount++;
            CompletionCallback?.Invoke(resource);

            return false;
        }

        private void ValidateUsage(ResourceDX11 src)
        {
            if (src.UsageFlags != Usage.Default &&
                src.UsageFlags != Usage.Immutable)
                throw new Exception("The current resource must have a usage flag of Default or Immutable. Only these flags allow the GPU read access for copying/reading data from the buffer.");

            if (DestResource.UsageFlags != Usage.Default)
                throw new Exception("The destination resource must have a usage flag of Staging or Default. Only these two allow the GPU write access for copying/writing data to the destination.");
        }
    }
}
