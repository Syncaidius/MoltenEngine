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

        protected override void OnInitializeAdapter(GraphicsSettings settings)
        {
            NativeWindow dummyWindow = new NativeWindow();
            _displayManager = new DisplayManagerGL();
            _displayManager.Initialize(_log, settings);

            dummyWindow.Dispose();
        }

        protected override void OnInitialize(GraphicsSettings settings)
        {
            _profiler = new RenderProfiler();
            _outputSurfaces = new ThreadedList<ISwapChainSurface>();
        }

        protected override void OnDispose()
        {
            _displayManager.Dispose();
        }

        protected override SceneRenderData OnCreateRenderData()
        {
            throw new NotImplementedException();
        }

        protected override void OnPresent(Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnRebuildSurfaces(int requiredWidth, int requiredHeight)
        {
            throw new NotImplementedException();
        }

        protected override void OnPrePresent(Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPostPresent(Timing time)
        {
            throw new NotImplementedException();
        }

        public string Namer => null;

        public RenderProfiler Profiler => null;

        public override IComputeManager Compute => null;

        public override string Name => "OpenGL Renderer";

        public override IDisplayManager DisplayManager => throw new NotImplementedException();

        public override IResourceManager Resources => throw new NotImplementedException();
    }
}
