using Molten.IO;

namespace Molten.Graphics
{
    internal struct BufferGetStreamTask : IGraphicsResourceTask
    {
        internal uint ByteOffset;

        internal GraphicsMapType MapType;

        internal GraphicsBuffer Staging;

        /// <summary>A callback to interact with the retrieved stream.</summary>
        internal Action<GraphicsBuffer, GraphicsStream> StreamCallback;

        public bool Process(GraphicsCommandQueue cmd, GraphicsResource resource)
        {
            using (GraphicsStream stream = cmd.MapResource(resource, 0, ByteOffset, MapType))
                StreamCallback?.Invoke(resource as GraphicsBuffer, stream);
  
            return false;
        }
    }
}
