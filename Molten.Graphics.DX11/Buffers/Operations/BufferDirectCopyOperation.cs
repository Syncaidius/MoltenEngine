namespace Molten.Graphics
{
    internal struct BufferDirectCopyOperation : IBufferOperation
    {
        internal GraphicsBuffer SourceBuffer;

        internal GraphicsBuffer DestinationBuffer;

        internal Action CompletionCallback;

        public void Process(DeviceContext pipe)
        {
            SourceBuffer.CopyTo(pipe, DestinationBuffer);
            CompletionCallback?.Invoke();
        }
    }
}
