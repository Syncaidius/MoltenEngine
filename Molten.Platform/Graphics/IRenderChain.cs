using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface IRenderChain
    {
        void Build(SceneRenderData sceneData, LayerRenderData layerData, RenderCamera camera);

        void Render(SceneRenderData sceneData, LayerRenderData layerData, RenderCamera camera, Timing time);
    }
}
