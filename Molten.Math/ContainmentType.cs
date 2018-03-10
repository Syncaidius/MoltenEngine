using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    /// <summary>
    /// Describes how one bounding volume contains another.
    /// </summary>
    public enum ContainmentType
    {
        /// <summary>
        /// The two bounding volumes don't intersect at all.
        /// </summary>
        Disjoint,

        /// <summary>
        /// One bounding volume completely contains another.
        /// </summary>
        Contains,

        /// <summary>
        /// The two bounding volumes overlap.
        /// </summary>
        Intersects
    }
}
