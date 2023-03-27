using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class GraphicsBuffer : GraphicsResource
    {
        protected GraphicsBuffer(GraphicsDevice device, uint stride, uint numElements, GraphicsResourceFlags flags, GraphicsBufferType type) :
            base(device, flags)
        {
            BufferType = type;
            Stride = stride;
            ElementCount = numElements;
            SizeInBytes = stride * numElements;
        }

        public abstract void SetData<T>(GraphicsPriority priority, T[] data, bool discard, GraphicsBuffer staging = null, Action completeCallback = null)
    where T : unmanaged;

        public abstract void SetData<T>(GraphicsPriority priority, T[] data, uint startIndex, uint elementCount, bool discard, uint byteOffset = 0, GraphicsBuffer staging = null, Action completeCallback = null)
            where T : unmanaged;

        public abstract void GetData<T>(GraphicsPriority priority, T[] destination, uint startIndex, uint count, uint elementOffset, Action<T[]> completionCallback = null)
            where T : unmanaged;

        public abstract void CopyTo(GraphicsPriority priority, GraphicsBuffer destination, Action<GraphicsResource> completionCallback = null);

        public abstract void CopyTo(GraphicsPriority priority, GraphicsBuffer destination, ResourceRegion sourceRegion, uint destByteOffset = 0, Action<GraphicsResource> completionCallback = null);

        public abstract void GetStream(GraphicsPriority priority, GraphicsMapType mapType, Action<GraphicsBuffer, GraphicsStream> callback, GraphicsBuffer staging = null);

        /// <summary>
        /// Gets the stride (byte size) of each element within the current <see cref="GraphicsBuffer"/>.
        /// </summary>
        public uint Stride { get; }

        /// <summary>
        /// Gets the number of elements that the current <see cref="GraphicsBuffer"/> can store.
        /// </summary>
        public uint ElementCount { get; }

        /// <summary>
        /// Gets the total size of the buffer, in bytes.
        /// </summary>
        public override uint SizeInBytes { get; }

        /// <summary>
        /// Gets the type of the current <see cref="GraphicsBuffer"/>.
        /// </summary>
        public GraphicsBufferType BufferType { get; }

        /// <summary>
        /// Gets the vertex format of the current <see cref="GraphicsBuffer"/>, if any.
        /// <para>This property is only set if the current <see cref="BufferType"/> is <see cref="GraphicsBufferType.Vertex"/>.</para>
        /// </summary>
        public VertexFormat VertexFormat { get; protected set; }

        /// <summary>
        /// Gets the <see cref="IndexBufferFormat"/> of the current <see cref="GraphicsBuffer"/>.
        /// <para>This property is only set if the current <see cref="BufferType"/> is <see cref="GraphicsBufferType.Index"/>.</para>
        /// </summary>
        public IndexBufferFormat IndexFormat { get; protected set; }
    }
}
