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
        /// Renders the 3D scene via the deferred rendering chain.
        /// </summary>
        Deferred = 1 << 3,

        /// <summary>
        /// Prevents the renderer from drawing a debug overlay in the scene's output surface.
        /// </summary>
        NoDebugOverlay = 1 << 4,
    }
}
