using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Serves one purpose only: To load a renderer library.</summary>
    public class RenderLoader
    {
        Assembly _renderAssembly;

        public IRenderer GetRenderer(Logger log, GraphicsSettings settings)
        {
            string defaultRenderer = GraphicsSettings.RENDERER_DX11; // TODO default to OpenGL if on a non-windows platform.
            IRenderer renderer = LibraryDetection.LoadInstance<IRenderer>(log, "render", "renderer", settings.RendererLibrary, settings, defaultRenderer, out _renderAssembly);

            // Initialize
            try
            {
                renderer.InitializeAdapter(settings);
                log.WriteLine($"Initialized renderer");
            }
            catch (Exception e)
            {
                log.WriteLine("Failed to initialize renderer");
                log.WriteError(e);
            }

            return renderer;
        }
    }
}
