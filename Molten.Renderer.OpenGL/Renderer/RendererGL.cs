using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Collections;

namespace Molten.Graphics
{
    public class RendererGL : MoltenRenderer
    {
        DisplayManagerGL _displayManager;
        ResourceManagerGL _resourceManager;

        protected override void OnInitializeAdapter(GraphicsSettings settings)
        {
            _displayManager = new DisplayManagerGL();
            _displayManager.Initialize(Log, settings);
        }

        protected override void OnInitialize(GraphicsSettings settings)
        {
            Device = new DeviceGL(Log, settings, _displayManager, settings.EnableDebugLayer);
            _resourceManager = new ResourceManagerGL(this);
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

        public override IDisplayManager DisplayManager => _displayManager;

        public override IResourceManager Resources => _resourceManager;

        internal DeviceGL Device { get; private set; }
    }
}
