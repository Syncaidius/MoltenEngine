namespace Molten.Graphics
{
    /// <summary>Represents the index format. This can either be a 32-bit or 16-bit unsigned value.</summary>
    public enum IndexBufferFormat
    {
        /// <summary>
        /// No index buffer format.
        /// </summary>
        None = 0,

        /// <summary>A unsigned 32-bit integer (uint).</summary>
        UInt32 = 1,

        /// <summary>A unsigned 16-bit integer (short).</summary>
        UInt16 = 2,
    }
}
