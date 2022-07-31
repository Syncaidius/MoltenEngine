using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public enum ContentHandleStatus
    {
        /// <summary>
        /// The content handle has not started processing yet. 
        /// </summary>
        NotProcessed = 0,

        /// <summary>
        /// The content handle is currently being processed.
        /// </summary>
        Processing = 1,

        /// <summary>
        /// The multi-part content handle is waiting for parts handles to finish processing.
        /// </summary>
        AwaitingParts = 2,

        /// <summary>
        /// The content handle has completed processing.
        /// </summary>
        Completed = 3,

        /// <summary>
        /// The content was unloaded by a <see cref="ContentManager"/>.
        /// </summary>
        Unloaded = 4,
    }
}
