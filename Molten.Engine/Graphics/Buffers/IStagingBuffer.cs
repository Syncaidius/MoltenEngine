namespace Molten.Graphics
{
    /// <summary>
    /// Represents the implementation of a staging buffer.
    /// </summary>
    public interface IStagingBuffer : IGraphicsBuffer
    {
        /// <summary>
        /// Gets the staging buffer access flags.
        /// </summary>
        StagingBufferFlags StagingType { get; }
    }
}
