namespace Molten.Graphics
{
    /// <summary>
    /// Represents the capabilities and limits of a type of buffer resource.
    /// </summary>
    public class BufferCapabilities
    {
        internal BufferCapabilities() { }

        /// <summary>
        /// Gets or sets the max number of elements for this buffer type.
        /// </summary>
        public uint MaxElements { get; set; }

        /// <summary>
        /// Gets or sets the max number of bytes for this buffer type.
        /// </summary>
        public uint MaxBytes { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of available slots for this type of buffer. 
        /// <para>This is not a shader stage-specific limit, but overall device limit. Some graphics APIs may not use per-stage limits.</para>
        /// </summary>
        public uint MaxSlots { get; set; }
    }
}
