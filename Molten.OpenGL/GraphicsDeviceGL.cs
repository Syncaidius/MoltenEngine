using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class GraphicsDeviceGL : EngineObject
    {
        GraphicsSettings _settings;
        GraphicsAdapterGL _adapter;
        DisplayManagerGL _displayManager;
        Logger _log;
        GraphicsContext _context;
        NativeWindow _dummyWindow;
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

            _dummyWindow = new NativeWindow();
            _context = new GraphicsContext(GraphicsMode.Default, _dummyWindow.WindowInfo);
        }

        internal void MakeCurrent(NativeWindow window)
        {
            _context.MakeCurrent(window.WindowInfo);
        }

        protected override void OnDispose()
        {
            DisposeObject(ref _context);
            DisposeObject(ref _dummyWindow);
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

        /// <summary>
        /// Gets the device context.
        /// </summary>
        internal IGraphicsContext Context => _displayManager.Context;
    }
}
