namespace Molten.Graphics
{
    internal struct BufferDirectCopyOperation : IBufferOperation
    {
        internal GraphicsBuffer SourceBuffer;

        internal GraphicsBuffer DestinationBuffer;

        internal Action CompletionCallback;

        public void Process(GraphicsCommandQueue cmd)
        {
            SourceBuffer.CopyTo(cmd, DestinationBuffer);
            CompletionCallback?.Invoke();
        }
    }
}
