using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class RenderChainGL : IRenderChain
    {
        RendererGL _renderer;

        internal RenderChainGL(RendererGL renderer)
        {
            _renderer = renderer;
        }

        public void Build(SceneRenderData sceneData, LayerRenderData layerData, RenderCamera camera)
        {
            //throw new NotImplementedException();
        }

        public void Render(SceneRenderData sceneData, LayerRenderData layerData, RenderCamera camera, Timing time)
        {
            //throw new NotImplementedException();
        }
    }
}
