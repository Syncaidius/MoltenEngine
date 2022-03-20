namespace Molten.Graphics
{
    public enum GraphicsContextType
    {
        /// <summary>All commands issued to the <see cref="IGpuContext"/> will be sent to the GPU immediately. 
        /// There is generally only one of these per <see cref="IGraphicsDevice"/> instance.</summary>
        Immediate = 0,

        /// <summary>The <see cref="IGpuContext"/> is deferred in some way to allow commands to be queued then submitted at a later point in time.</summary>
        Deferred = 1,
    }
}
