namespace Molten.Graphics
{
    internal struct BufferDirectCopyOperation : IBufferOperation
    {
        internal GraphicsBuffer SourceBuffer;

        internal GraphicsBuffer DestinationBuffer;

        internal Action CompletionCallback;

        public void Process(DeviceContext context)
        {
            SourceBuffer.CopyTo(context, DestinationBuffer);
            CompletionCallback?.Invoke();
        }
    }
}
