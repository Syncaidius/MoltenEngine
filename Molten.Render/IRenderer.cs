using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface IRenderer : IDisposable
    {
        void InitializeAdapter(GraphicsSettings settings);

        void InitializeRenderer(GraphicsSettings settings);

        void Present(Timing time);

        /// <summary>Gets the display manager bound to the renderer.</summary>
        IDisplayManager DisplayManager { get; }

        /// <summary>Gets the resource manager bound to the renderer. 
        /// This is responsible for creating and destroying graphics resources, such as buffers, textures and surfaces.</summary>
        IResourceManager Resources { get; }

        IMaterialManager Materials { get; }

        IComputeManager Compute { get; }

        /// <summary>Gets the name of the renderer.</summary>
        string Name { get; }

        /// <summary>Gets profiling data attached to the renderer.</summary>
        IRenderProfiler Profiler { get; }

        /// <summary>Gets a list of <see cref="ISwapChainSurface"/> objects which are presented to display devices. This list is safe to modify from any thread.</summary>
        ThreadedList<ISwapChainSurface> OutputSurfaces { get; }

        /// <summary>Gets or sets the default <see cref="IRenderSurface"/>. This is used when objects such as a <see cref="Scene"/> do not have a render surface set on them.</summary>
        IRenderSurface DefaultSurface { get; set; }

        SceneRenderData CreateRenderData();

        void DestroyRenderData(SceneRenderData data);

        /// <summary>
        /// brings a scene to the front. This means it will be rendered (last) in front of all other scenes.
        /// </summary>
        /// <param name="data">The scene's render data instance.</param>
        void BringToFront(SceneRenderData data);
        
        /// <summary>
        /// Sends a scene to the back. This means it will be rendered (first) behind all other scenes.
        /// </summary>
        /// <param name="data">The scene's render data instance.</param>
        void SendToBack(SceneRenderData data);
        
        /// <summary>
        /// Swaps a scene's render order with the scene in front it (if any).
        /// </summary>
        /// <param name="data">The scene's render data instance.</param>
        void PushForward(SceneRenderData data);

        /// <summary>
        /// Swaps a scene's render order with the scene behind it (if any).
        /// </summary>
        /// <param name="data">The scene's render data instance.</param>
        void PushBackward(SceneRenderData data);

        /// <summary>
        /// Retrieves a new render debug overlay instance, which can optionally be set to display statistics for a scene initially.
        /// </summary>
        /// <param name="font">The <see cref="SpriteFont"/> to use when drawing the debug overlay.</param>
        /// <param name="initialScene">The scene to display statistics/information for after creation of the debug overlay. Can be null.</param>
        /// <returns></returns>
        ISceneDebugOverlay GetDebugOverlay(SceneRenderData initialScene = null);
    }
}
