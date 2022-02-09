namespace Molten.Graphics
{
    public enum BufferMode
    {
        /// <summary>The buffer cannot be written to, or read from by the CPU.
        /// Once the first set of data has been set, static buffers update their data by recreating the internal buffer. 
        /// This is an extremely expensive operation.
        /// To avoid this cost, use a staging buffer to copy data into buffers created with this mode.</summary>
        Default = 0,

        /// <summary>The buffer cannot be read or written to by the GPU (or written to by the GPU) after creation. Immutable buffers
        /// perform faster than all other types on the GPU and provide an opportunity for the GPU drivers to optimize 
        /// access/draw/threaded calls.
        /// 
        /// Once the first set of data has been set, immutable buffers update their data by recreating the internal buffer. 
        /// This is an extremely expensive operation.</summary>
        Immutable = 1,

        /// <summary>A dynamic buffer can be written to (but not read from) by the CPU. Useful for data that will change every frame. <para/>
        /// In this mode, every map call will discard the buffer on the GPU and be pointed to a new area of VRAM.</summary>
        DynamicDiscard = 2,

        /// <summary>A dynamic buffer can be written to (but not read from) by the CPU. Useful for data that will change every frame. <para/>
        /// In this mode, each map call will be pointed to an uninitialized/unwritten area of buffer memory. 
        /// Once the buffer is full, the next map call will discard the buffer on the GPU and point to a new area of VRAM.
        /// This cycle of filling and discarding ensures dynamic data can always be written.</summary>
        DynamicRing = 3,
    }
}
