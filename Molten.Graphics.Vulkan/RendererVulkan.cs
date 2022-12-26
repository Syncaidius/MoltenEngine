using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class RendererVK : RenderService
    {
        DisplayManagerVK _displayManager;

        public RendererVK()
        {
            _displayManager = new DisplayManagerVK();
        }

        public override IDisplayManager DisplayManager => _displayManager;

        public override ResourceFactory Resources => throw new NotImplementedException();

        public override IComputeManager Compute => throw new NotImplementedException();

        protected override IRenderChain GetRenderChain()
        {
            throw new NotImplementedException();
        }

        protected override SceneRenderData OnCreateRenderData()
        {
            throw new NotImplementedException();
        }

        protected override void OnDisposeBeforeRender()
        {
            throw new NotImplementedException();
        }

        

        protected override void OnPostPresent(Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPostRenderCamera(SceneRenderData sceneData, RenderCamera camera, Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPostRenderScene(SceneRenderData sceneData, Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPrePresent(Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPreRenderCamera(SceneRenderData sceneData, RenderCamera camera, Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPreRenderScene(SceneRenderData sceneData, Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnRebuildSurfaces(uint requiredWidth, uint requiredHeight)
        {
            throw new NotImplementedException();
        }
    }
}
