using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal struct BufferCopyOperation : IBufferOperation
    {
        internal GraphicsBuffer SourceBuffer;

        internal GraphicsBuffer DestinationBuffer;

        internal Box SourceRegion;

        /// <summary>The number of bytes to offset the data in the <see cref="DestinationBuffer"/>.</summary>
        internal uint DestinationByteOffset;

        /// <summary>The number of bytes per element in <see cref="Data"/>.</summary>
        public uint DataStride;

        internal Action CompletionCallback;

        public void Process(DeviceContext context)
        {
            CompletionCallback?.Invoke();
        }
    }
}
