using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Collections;
using Molten.Graphics.OpenGL;
using OpenTK;

namespace Molten.Graphics
{
    public class RendererGL : MoltenRenderer
    {
        DisplayManagerGL _displayManager;

        protected override void OnInitializeAdapter(GraphicsSettings settings)
        {
            NativeWindow dummyWindow = new NativeWindow();
            _displayManager = new DisplayManagerGL();
            _displayManager.Initialize(Log, settings);

            dummyWindow.Dispose();
        }

        protected override void OnInitialize(GraphicsSettings settings)
        {
            Device = new GraphicsDeviceGL(Log, settings, Profiler, _displayManager, settings.EnableDebugLayer);
        }

        protected override void OnDispose()
        {
            Device?.Dispose();
            _displayManager?.Dispose();
        }

        protected override SceneRenderData OnCreateRenderData()
        {
            throw new NotImplementedException();
        }

        protected override IRenderChain GetRenderChain()
        {
            return new RenderChainGL(this);
        }

        protected override void OnPreRenderScene(SceneRenderData sceneData, Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPostRenderScene(SceneRenderData sceneData, Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPreRenderCamera(SceneRenderData sceneData, RenderCamera camera, Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPostRenderCamera(SceneRenderData sceneData, RenderCamera camera, Timing time)
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

        public override IComputeManager Compute => null;

        public override string Name => "OpenGL";

        public override IDisplayManager DisplayManager => throw new NotImplementedException();

        public override IResourceManager Resources => throw new NotImplementedException();

        internal GraphicsDeviceGL Device { get; private set; }
    }
}
