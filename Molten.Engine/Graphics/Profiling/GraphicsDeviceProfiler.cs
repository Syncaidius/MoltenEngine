namespace Molten.Graphics
{
    public class GraphicsDeviceProfiler : GraphicsQueueProfiler
    {
        internal void Accumulate(GraphicsDeviceProfiler profiler)
        {
            base.Accumulate(profiler);

            VideoMemoryAllocated += profiler.VideoMemoryAllocated;
            VideoMemoryDeallocated += profiler.VideoMemoryDeallocated;
            VideoMemoryUsed += profiler.VideoMemoryUsed;
        }

        /// <summary>
        /// Gets or sets the total amount of VRAM allocated by the device, in bytes.
        /// </summary>
        public ulong VideoMemoryAllocated { get; set; }

        /// <summary>
        /// Gets or sets the total amount of VRAM deallocated by the device, in bytes.
        /// </summary>
        public ulong VideoMemoryDeallocated { get; set; }

        /// <summary>
        /// Gets or sets the total amount of VRAM used by the device, in bytes.
        /// </summary>
        public ulong VideoMemoryUsed { get; set; }
    }
}
