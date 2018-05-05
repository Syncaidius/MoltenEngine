using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class GraphicsDeviceGL : GraphicsPipeGL
    {
        GraphicsSettings _settings;
        GraphicsAdapterGL _adapter;
        DisplayManagerGL _displayManager;
        Logger _log;

        internal GraphicsDeviceGL(Logger log, GraphicsSettings settings, RenderProfiler profiler, DisplayManagerGL manager, bool enableDebugLayer)
        {
            _log = log;
            _displayManager = manager;
            _adapter = _displayManager.SelectedAdapter as GraphicsAdapterGL;
            _settings = settings;
            Features = new GraphicsOpenGLFeatures();
        }

        /// <summary>Gets the hardware features supported by the current device.</summary>
        internal GraphicsOpenGLFeatures Features { get; private set; }
    }
}
