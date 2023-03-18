using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal struct BufferCopyOperation : IGraphicsResourceTask
    {

        internal BufferDX11 DestBuffer;

        internal Box SrcRegion;

        /// <summary>The number of bytes to offset the data in the <see cref="DestBuffer"/>.</summary>
        internal uint DestByteOffset;

        internal Action CompletionCallback;

        public unsafe void Process(GraphicsCommandQueue cmd, GraphicsResource resource)
        {
            BufferDX11 srcBuffer = resource as BufferDX11;
            ValidateCopyBufferUsage(srcBuffer);

            // If the current buffer is a staging buffer, initialize and apply all its pending changes.
            if (srcBuffer.Desc.Usage == Usage.Staging)
                srcBuffer.Apply(cmd);

            (cmd as CommandQueueDX11).CopyResourceRegion(srcBuffer, 0, ref SrcRegion, DestBuffer, 0, new Vector3UI(DestByteOffset, 0, 0));
            cmd.Profiler.Current.CopySubresourceCount++;
            CompletionCallback?.Invoke();
        }

        private void ValidateCopyBufferUsage(BufferDX11 srcBuffer)
        {
            if (srcBuffer.Desc.Usage != Usage.Default &&
                srcBuffer.Desc.Usage != Usage.Immutable)
                throw new Exception("The current buffer must have a usage flag of Default or Immutable. Only these flags allow the GPU read access for copying/reading data from the buffer.");

            if (DestBuffer.Desc.Usage != Usage.Default)
                throw new Exception("The destination buffer must have a usage flag of Staging or Default. Only these two allow the GPU write access for copying/writing data to the destination.");
        }
    }
}
