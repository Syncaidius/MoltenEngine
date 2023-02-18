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
        Unsigned32Bit = 1,

        /// <summary>A unsigned 16-bit integer (short).</summary>
        Unsigned16Bit = 2,
    }
}
