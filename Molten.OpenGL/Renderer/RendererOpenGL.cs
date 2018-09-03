using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Collections;
using OpenTK;

namespace Molten.Graphics
{
    public class RendererOpenGL : RenderEngine
    {
        ThreadedList<ISwapChainSurface> _outputSurfaces;
        RenderProfiler _profiler;
        Logger _log;
        DisplayManagerGL _displayManager;

        public RendererOpenGL()
        {
            _log = Logger.Get();
            _log.AddOutput(new LogFileWriter("renderer_opengl{0}.txt"));
        }

        public override void Initialize(GraphicsSettings settings)
        {
            _profiler = new RenderProfiler();
            _outputSurfaces = new ThreadedList<ISwapChainSurface>();
        }

        public override void InitializeAdapter(GraphicsSettings settings)
        {
            NativeWindow dummyWindow = new NativeWindow();
            _displayManager = new DisplayManagerGL();
            _displayManager.Initialize(_log, settings);

            dummyWindow.Dispose();
        }

        public override void Dispose()
        {
            _outputSurfaces.ForInterlock(0, 1, (index, surface) =>
            {
                surface.Dispose();
                return false;
            });

            _displayManager.Dispose();
            _log?.Dispose();

        }

        public void Present(Timing time)
        {
            
        }

        protected override SceneRenderData OnCreateRenderData()
        {
            throw new NotImplementedException();
        }

        protected override void OnPresent(Timing time)
        {
            throw new NotImplementedException();
        }

        public string Namer => null;

        public RenderProfiler Profiler => null;

        public override IComputeManager Compute => null;

        public override string Name => "OpenGL Renderer";

        public override IDisplayManager DisplayManager => throw new NotImplementedException();

        public override IResourceManager Resources => throw new NotImplementedException();

        public override IRenderSurface DefaultSurface { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override ThreadedList<ISwapChainSurface> OutputSurfaces => throw new NotImplementedException();
    }
}
