using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Collections;
using OpenTK;

namespace Molten.Graphics
{
    public class RendererOpenGL : IRenderer
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

        public void Initialize(GraphicsSettings settings)
        {
            _profiler = new RenderProfiler();
            _outputSurfaces = new ThreadedList<ISwapChainSurface>();
        }

        public void InitializeAdapter(GraphicsSettings settings)
        {
            GameWindow dummyWindow = new GameWindow();
            _displayManager = new DisplayManagerGL();
            _displayManager.Initialize(_log, settings);

            dummyWindow.Dispose();
        }

        public void BringToFront(SceneRenderData data)
        {
            
        }

        public void SendToBack(SceneRenderData data)
        {
            
        }

        public void PushBackward(SceneRenderData data)
        {
            
        }

        public void PushForward(SceneRenderData data)
        {
            
        }

        public SceneRenderData CreateRenderData()
        {
            return null;
        }

        public void DestroyRenderData(SceneRenderData data)
        {
            
        }

        public void Dispose()
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

        public IDisplayManager DisplayManager => null;

        public string Namer => null;

        public RenderProfiler Profiler => null;

        public ThreadedList<ISwapChainSurface> OutputSurfacesr => null;

        public IRenderSurface DefaultSurface { get; set; }

        public IResourceManager Resources => null;

        public IMaterialManager Materials => null;

        public IComputeManager Compute => null;

        public string Name => "OpenGL Renderer";

        public ThreadedList<ISwapChainSurface> OutputSurfaces => _outputSurfaces;
    }
}
