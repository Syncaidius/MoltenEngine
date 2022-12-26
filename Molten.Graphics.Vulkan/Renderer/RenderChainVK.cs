using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class RenderChainVK : IRenderChain
    {
        RendererVK _renderer;
        internal RenderChainVK(RendererVK renderer)
        {
            _renderer = renderer;
        }

        public void Build(SceneRenderData sceneData, LayerRenderData layerData, RenderCamera camera)
        {
            
        }

        public void Render(SceneRenderData sceneData, LayerRenderData layerData, RenderCamera camera, Timing time)
        {
            
        }
    }
}
