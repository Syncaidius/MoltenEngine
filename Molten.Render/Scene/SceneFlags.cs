using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// A set of flags which define basic rendering rules for a scene.
    /// </summary>
    [Flags]
    public enum SceneRenderFlags
    {
        None = 0,

        /// <summary>
        /// Do not clear the output surface of the scene before rendering it.
        /// </summary>
        DoNotClear = 1,

        /// <summary>
        /// Render 2D scene objects
        /// </summary>
        TwoD = 1 << 1,

        /// <summary>
        /// Render 3D scene objects.
        /// </summary>
        ThreeD = 1 << 2,

        /// <summary>
        /// Render both 2D and 3D scene objects.
        /// </summary>
        TwoAndThreeD = TwoD | ThreeD,
    }
}
