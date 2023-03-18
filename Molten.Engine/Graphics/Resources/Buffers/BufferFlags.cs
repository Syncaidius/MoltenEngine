namespace Molten.Graphics
{
    [Flags]
    public enum BufferFlags
    {
        /// <summary>
        /// No flags. This is usually invalid.
        /// </summary>
        None = 0,

        /// <summary>
        /// Allow the CPU to read from the buffer. Generally, only staging buffers allow the CPU to map and read them.
        /// </summary>
        CpuRead = 1,

        /// <summary>
        /// Allow the CPU to write to the buffer.
        /// </summary>
        CpuWrite = 1 << 1,

        /// <summary>
        /// Allow the GPU to read from the buffer.
        /// </summary>
        GpuRead = 1 << 2,

        /// <summary>
        /// Allow the GPU to write to the buffer.
        /// </summary>
        GpuWrite = 1 << 3,

        /// <summary>
        /// Each time data is uploaded to the buffer, a new section of the buffer's memory will be used. 
        /// Once the end of the buffer has insufficient space, it will cycle back around to the start of the buffer. This forms the ring.
        /// </summary>
        Ring = 1 << 4,

        /// <summary>
        /// Each data is uploaded to the buffer, the current buffer memory is discarded after the GPU has finished with it. New area of memory is allocated to accomodate the new data.
        /// </summary>
        Discard = 1 << 5,
    }

    public static class BufferFlagsExtensions
    {
        public static bool HasFlags(this BufferFlags flags, BufferFlags value)
        {
            return (flags & value) == value;
        }

        public static bool IsImmutable(this BufferFlags flags)
        {
            return (flags & BufferFlags.GpuRead) == BufferFlags.GpuRead &&
                (flags & BufferFlags.GpuWrite) != BufferFlags.GpuWrite &&
                (flags & BufferFlags.CpuRead) != BufferFlags.CpuRead &&
                (flags & BufferFlags.CpuWrite) != BufferFlags.CpuWrite;
        }
    }
}
