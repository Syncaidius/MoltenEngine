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

        /// <summary>Sets the current debug overlay page. Returns the ID of the next page.</summary>
        /// <param name="visible">If true, the debug overlay will be visible.</param>
        /// <param name="page">The page number.</param>
        /// <returns></returns>
        int SetDebugOverlayPage(bool visible, int page);

        /// <summary>Gets the time taken to process the previous frame.</summary>
        TimeSpan FrameTime { get; }
    }
}
