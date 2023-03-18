using Molten.IO;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal struct BufferGetOperation<T> : IGraphicsResourceTask 
        where T : unmanaged
    {
        /// <summary>The number of bytes to offset the change, from the start of the provided <see cref="SrcSegment"/>.</summary>
        internal uint ByteOffset;

        /// <summary>The number of bytes per element in <see cref="Data"/>.</summary>
        internal uint DataStride;

        /// <summary>The number of elements to be copied.</summary>
        internal uint Count;

        /// <summary>The first index at which to start placing the retrieved data within <see cref="DestArray"/>.</summary>
        internal uint DestIndex;

        internal BufferDX11 SrcBuffer;

        /// <summary>A callback to send the retrieved data to.</summary>
        internal Action<T[]> CompletionCallback;

        /// <summary>The destination array to store the retrieved data.</summary>
        internal T[] DestArray;

        public unsafe void Process(GraphicsCommandQueue cmd)
        {
            CommandQueueDX11 dx11Cmd = cmd as CommandQueueDX11;
            DestArray = DestArray ?? new T[Count];

            // Now set the structured variable's data
            MappedSubresource dataBox = dx11Cmd.MapResource(SrcBuffer.ResourcePtr, 0, Map.Read, 0, out RawStream stream);
            cmd.Profiler.Current.MapReadCount++;
            stream.Position = ByteOffset;
            stream.ReadRange(DestArray, 0, Count);
            dx11Cmd.UnmapResource(SrcBuffer.ResourcePtr, 0);

            CompletionCallback?.Invoke(DestArray);
        }
    }
}
