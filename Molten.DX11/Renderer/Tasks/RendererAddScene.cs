using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="SceneObject"/> to the root of a scene.</summary>
    internal class RendererAddScene : RendererTask<RendererAddScene>
    {
        public SceneRenderDataDX11 Data;

        public override void Clear()
        {
            Data = null;
        }

        public override void Process(RendererDX11 renderer)
        {
            renderer.Scenes.Add(Data);
            Recycle(this);
        }
    }
}
