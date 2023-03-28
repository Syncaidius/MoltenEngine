using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="priority"></param>
        /// <param name="data"></param>
        /// <param name="staging"></param>
        /// <param name="completeCallback"></param>
        public void SetData<T>(GraphicsPriority priority, T[] data, bool discard, GraphicsBuffer staging = null, Action completeCallback = null)
    where T : unmanaged
        {
            SetData(priority, data, 0, (uint)data.Length, discard, 0, staging, completeCallback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="priority"></param>
        /// <param name="data"></param>
        /// <param name="startIndex">The start index within <paramref name="data"/> to copy.</param>
        /// <param name="elementCount"></param>
        /// <param name="byteOffset">The start location within the buffer to start copying from, in bytes.</param>
        /// <param name="staging"></param>
        /// <param name="completeCallback"></param>
        /// <param name="discard">If true, the previous data will be discarded. Ignored if not applicable to the current buffer.</param>
        public abstract void SetData<T>(GraphicsPriority priority, T[] data, uint startIndex, uint elementCount, bool discard, uint byteOffset = 0, GraphicsBuffer staging = null, Action completeCallback = null)
            where T : unmanaged;

        /// <summary>Retrieves data from a <see cref="GraphicsBuffer"/>.</summary>
        /// <param name="destination">The destination array. Must be big enough to contain the retrieved data.</param>
        /// <param name="startIndex">The start index within the destination array at which to place the retrieved data.</param>
        /// <param name="count">The number of elements to retrieve</param>
        /// <param name="byteOffset">The start location within the buffer to start copying from, in bytes.</param>
        /// <param name="completionCallback">A callback to run once the operation is completed.</param>
        public void GetData<T>(GraphicsPriority priority, T[] destination, uint startIndex, uint count, uint byteOffset, Action<T[]> completionCallback = null)
            where T : unmanaged
        {
            if (Flags.Has(GraphicsResourceFlags.CpuRead))
                throw new GraphicsResourceException(this, "Cannot use GetData() on a non-readable buffer.");

            if (destination.Length < count)
                throw new ArgumentException("The provided destination array is not large enough.");

            QueueTask(priority, new BufferGetTask<T>()
            {
                ByteOffset = byteOffset,
                DestArray = destination,
                DestIndex = startIndex,
                Count = count,
                MapType = GraphicsMapType.Read,
                CompletionCallback = completionCallback,
            });
        }

        public abstract void CopyTo(GraphicsPriority priority, GraphicsBuffer destination, Action<GraphicsResource> completionCallback = null);

        public abstract void CopyTo(GraphicsPriority priority, GraphicsBuffer destination, ResourceRegion sourceRegion, uint destByteOffset = 0, Action<GraphicsResource> completionCallback = null);

        public void GetStream(GraphicsPriority priority, GraphicsMapType mapType, Action<GraphicsBuffer, GraphicsStream> callback, GraphicsBuffer staging = null)
        {
            QueueTask(priority, new BufferGetStreamTask()
            {
                ByteOffset = 0,
                Staging = staging,
                StreamCallback = callback,
                MapType = mapType,
            });
        }

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
