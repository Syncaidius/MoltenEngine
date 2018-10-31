using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace Molten.Graphics
{
    internal class GraphicsDeviceGL : EngineObject
    {
        GraphicsSettings _settings;
        GraphicsAdapterGL _adapter;
        DisplayManagerGL _displayManager;
        Logger _log;
        RenderProfiler _profiler;
        RenderProfiler _defaultProfiler;

        internal GraphicsDeviceGL(Logger log, GraphicsSettings settings, RenderProfiler profiler, DisplayManagerGL manager, bool enableDebugLayer)
        {
            _log = log;
            _displayManager = manager;
            _adapter = _displayManager.SelectedAdapter as GraphicsAdapterGL;
            _settings = settings;
            _defaultProfiler = _profiler = new RenderProfiler();
            Features = new GraphicsOpenGLFeatures();
        }

        protected override void OnDispose()
        {
            base.OnDispose();
        }

        /// <summary>Gets the hardware features supported by the current device.</summary>
        internal GraphicsOpenGLFeatures Features { get; private set; }

        /// <summary>Gets the profiler bound to the current <see cref="GraphicsPipe"/>. Contains statistics for this pipe alone.</summary>
        internal RenderProfiler Profiler
        {
            get => _profiler;
            set => _profiler = value ?? _defaultProfiler;
        }
    }
}
