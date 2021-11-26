using OpenGL;
using System;

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

        uint _fboID;

        internal DeviceGL(Logger log, GraphicsSettings settings, DisplayManagerGL manager)
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
            _fboID = Gl.GenFramebuffer();
        }

        protected override void OnDispose()
        {
            if (_fboID > 0)
            {
                Gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fboID);
                Gl.DeleteFramebuffers(_fboID);
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
        internal uint FrameBufferID => _fboID;

        DeviceGL IGraphicsPipe<DeviceGL>.Device => this;

        RenderProfiler IGraphicsPipe<DeviceGL>.Profiler => _profiler;
    }
}
