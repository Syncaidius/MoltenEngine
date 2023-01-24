using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public enum SurfaceSizeMode : byte
    {
        /// <summary>
        /// The surface will be at least the width and height of the largest-rendered surface.
        /// </summary>
        Full = 0,

        /// <summary>
        /// The surface will be at least half the width and height of the largest-rendered surface.
        /// </summary>
        Half = 1,

        /// <summary>
        /// The surface will remain at a fixed size regardless of resolution changes.
        /// </summary>
        Fixed = 2,
    }
}
