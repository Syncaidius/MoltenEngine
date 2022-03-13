using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public enum RenderCameraMode
    {
        /// <summary>
        /// Configures a camera for left-handed perspective mode.
        /// </summary>
        Perspective = 0,

        /// <summary>
        /// Configures a camera for left-handed orthographic mode.
        /// </summary>
        Orthographic = 1,
    }
}
