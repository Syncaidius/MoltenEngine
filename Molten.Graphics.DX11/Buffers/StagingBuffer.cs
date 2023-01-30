namespace Molten.Graphics
{
    internal class StagingBuffer : GraphicsBuffer, IStagingBuffer
    {
        /// <summary>Creates a new instance of <see cref="StagingBuffer"/>.</summary>
        /// <param name="device">The graphics device to bind the buffer to.</param>
        /// <param name="stageType">Access flags for the buffer.</param>
        /// <param name="capacity">The number of elements the buffer should be able to hold.</param>
        internal StagingBuffer(DeviceDX11 device, StagingBufferFlags stageType, uint capacity)
            : base(device, BufferMode.Default, 0, capacity, 0, stageType)
        {
            StagingType = stageType;
        }

        /// <summary>Gets the staging mode of the buffer.</summary>
        public StagingBufferFlags StagingType { get; }
    }
}
