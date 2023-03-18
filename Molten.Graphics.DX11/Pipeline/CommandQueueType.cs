namespace Molten.Graphics
{
    public enum CommandQueueType
    {
        /// <summary>All commands issued to the <see cref="CommandQueueDX11"/> will be sent to the GPU immediately. 
        /// There is generally only one of these per <see cref="DeviceDX11"/> instance.</summary>
        Immediate = 0,

        /// <summary>The <see cref="CommandQueueDX11"/> is deferred in some way to allow commands to be queued then submitted at a later point in time.</summary>
        Deferred = 1,
    }
}
