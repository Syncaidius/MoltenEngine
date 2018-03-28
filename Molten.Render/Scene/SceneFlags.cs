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
        /// <summary>
        /// No flags. This will generally stop a scene from being rendered at all.
        /// </summary>
        None = 0,

        /// <summary>
        /// Do not clear the output surface of the scene before rendering it.
        /// </summary>
        DoNotClear = 1,

        /// <summary>
        /// Render 2D scene objects
        /// </summary>
        Render2D = 1 << 1,

        /// <summary>
        /// Render 3D scene objects.
        /// </summary>
        Render3D = 1 << 2,

        /// <summary>
        /// Renders the 3D scene via deferred rendering.
        /// </summary>
        Deferred = 1 << 3,

        /// <summary>
        /// Prevents the renderer from drawing a debug overlay in the scene's output surface.
        /// </summary>
        NoDebugOverlay = 1 << 4,

        /// <summary>
        /// Renders 2D scene objects behind post-processing, resulting in all 2D objects being affected by the scene's attached post-process effects.
        /// </summary>
        Render2DBeforePostProcess = 1 << 5,

        /// <summary>
        /// Skips post-processing on the current scene.
        /// </summary>
        NoPostProcessing = 1 << 6,

        /// <summary>Skips the lighting stage on the current scene. This will result in the scene being rendered full-bright (fully lit).</summary>
        NoLighting = 1 << 7,

        /// <summary>
        /// Skips the shadow-casting stage on the current scene, which will result in the final scene having no shadows.<para/>
        /// Overrides any graphics settings which enable shadows.
        /// </summary>
        NoShadows = 1 << 8,
    }
}
