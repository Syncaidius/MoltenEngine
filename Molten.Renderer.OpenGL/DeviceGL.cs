using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Molten.Graphics
{
    public class DeviceGL : EngineObject, IGraphicsDevice, IGraphicsPipe<DeviceGL>
    {
        GraphicsSettings _settings;
        GraphicsAdapterGL _adapter;
        DisplayManagerGL _displayManager;
        Logger _log;
        RenderProfiler _profiler;
        RenderProfiler _defaultProfiler;

        int _fboID;

        internal DeviceGL(Logger log, GraphicsSettings settings, DisplayManagerGL manager, bool enableDebugLayer)
        {
            _log = log;
            _displayManager = manager;
            _adapter = _displayManager.SelectedAdapter as GraphicsAdapterGL;
            _settings = settings;
            _defaultProfiler = _profiler = new RenderProfiler();
            Features = new GraphicsOpenGLFeatures();

            Initialize(log, settings);
        }

        private void Initialize(Logger log, GraphicsSettings settings)
        {
            _fboID = GL.GenFramebuffer();
        }

        protected override void OnDispose()
        {
            if (_fboID > 0)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fboID);
                GL.DeleteFramebuffer(_fboID);
            }
            base.OnDispose();
        }

        public void MarkForDisposal(PipelineDisposableObject obj)
        {
            throw new NotImplementedException();
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
        /// Gets the frame buffer object (FBO) ID.
        /// </summary>
        internal int FrameBufferID => _fboID;

        DeviceGL IGraphicsPipe<DeviceGL>.Device => this;

        RenderProfiler IGraphicsPipe<DeviceGL>.Profiler => _profiler;
    }
}
