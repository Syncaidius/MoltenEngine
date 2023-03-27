using Molten.IO;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal struct BufferGetTask<T> : IGraphicsResourceTask 
        where T : unmanaged
    {
        /// <summary>The number of bytes to offset the change, from the start of the provided <see cref="SrcSegment"/>.</summary>
        internal uint ByteOffset;
        /// <summary>The number of elements to be copied.</summary>
        internal uint Count;

        /// <summary>The first index at which to start placing the retrieved data within <see cref="DestArray"/>.</summary>
        internal uint DestIndex;

        internal GraphicsMapType MapType;

        /// <summary>A callback to send the retrieved data to.</summary>
        internal Action<T[]> CompletionCallback;

        /// <summary>The destination array to store the retrieved data.</summary>
        internal T[] DestArray;

        public unsafe bool Process(GraphicsCommandQueue cmd, GraphicsResource resource)
        {
            BufferDX11 srcBuffer = resource as BufferDX11;
            DestArray = DestArray ?? new T[Count];

            // Now set the structured variable's data
            using (GraphicsStream stream = cmd.MapResource(srcBuffer, 0, ByteOffset, MapType))
                stream.ReadRange(DestArray, DestIndex, Count);

            CompletionCallback?.Invoke(DestArray);
            return false;
        }
    }
}
