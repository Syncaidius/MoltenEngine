using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal struct BufferCopyOperation : IBufferOperation
    {
        internal GraphicsBuffer SrcBuffer;

        internal GraphicsBuffer DestBuffer;

        internal Box SrcRegion;

        /// <summary>The number of bytes to offset the data in the <see cref="DestBuffer"/>.</summary>
        internal uint DestByteOffset;

        internal Action CompletionCallback;

        public unsafe void Process(GraphicsCommandQueue cmd)
        {
            ValidateCopyBufferUsage();

            // If the current buffer is a staging buffer, initialize and apply all its pending changes.
            if (SrcBuffer.Desc.Usage == Usage.Staging)
                SrcBuffer.Apply(cmd);

            (cmd as CommandQueueDX11).CopyResourceRegion(SrcBuffer, 0, ref SrcRegion, DestBuffer, 0, new Vector3UI(DestByteOffset, 0, 0));
            cmd.Profiler.Current.CopySubresourceCount++;
            CompletionCallback?.Invoke();
        }

        private void ValidateCopyBufferUsage()
        {
            if (SrcBuffer.Desc.Usage != Usage.Default &&
                SrcBuffer.Desc.Usage != Usage.Immutable)
                throw new Exception("The current buffer must have a usage flag of Default or Immutable. Only these flags allow the GPU read access for copying/reading data from the buffer.");

            if (DestBuffer.Desc.Usage != Usage.Default)
                throw new Exception("The destination buffer must have a usage flag of Staging or Default. Only these two allow the GPU write access for copying/writing data to the destination.");
        }
    }
}
