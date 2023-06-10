using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    [Flags]
    public enum GraphicsCommandListFlags
    {
        /// <summary>
        /// No flags. The command list should only be submitted once per frame and will not provide any synchronization objects.
        /// </summary>
        None = 0,

        /// <summary>
        /// A short-lived/transient command list, intended for successive submissions within the same frame.
        /// </summary>
        Short = 1,

        /// <summary>
        /// The next command list will provide a fence for syncing.
        /// </summary>
        CpuSyncable = 1 << 1,

        /// <summary>
        /// The current command list will wait for all previously-submitted command lists in the current frame, which had the <see cref="CpuSyncable"/> flag set.
        /// </summary>
        CpuWait = 1 << 2,

        /// <summary>
        /// The current command list list can only be submitted once.
        /// </summary>
        SingleSubmit = 1 << 3,
        
        /// <summary>
        /// The current command list is not submitted once <see cref="GraphicsQueue.End"/> is called.
        /// </summary>
        Deferred = 1 << 4,
    }

    public static class GraphicsCommandListFlagsExtensions
    {
        public static bool Has(this GraphicsCommandListFlags flags, GraphicsCommandListFlags flag)
        {
            return (flags & flag) == flag;
        }
    }
}
