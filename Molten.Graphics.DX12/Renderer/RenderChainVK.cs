using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class RenderChainDX12 : IRenderChain
    {
        RendererDX12 _renderer;
        internal RenderChainDX12(RendererDX12 renderer)
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
