using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public enum GraphicsAdapterVendor
    {
        /// <summary>
        /// The vendor could not be determined.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Advanced Micro Devices.
        /// </summary>
        AMD = 1,

        /// <summary>
        /// Intel Corporation.
        /// </summary>
        Intel = 2,

        /// <summary>
        /// Nvidia Corporation.
        /// </summary>
        Nvidia = 3,
    }
}
