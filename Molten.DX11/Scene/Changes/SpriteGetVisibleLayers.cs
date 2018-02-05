using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="SceneObject"/> to the root of a scene.</summary>
    internal class SpriteGetVisibleLayers : RenderSceneChange<SpriteGetVisibleLayers> 
    {
        public List<int> Output;

        public Action<List<int>> RetrievalCallback;

        public override void Clear()
        {
            RetrievalCallback = null;
            Output = null;
        }

        public override void Process(SceneRenderDataDX11 scene)
        {
            Output.Clear();

            for(int i = 0; i < scene.SpriteLayers.Length; i++)
            {
                if (scene.SpriteLayers[i].Visible)
                    Output.Add(i);
            }

            RetrievalCallback?.Invoke(Output);
            Recycle(this);
        }
    }
}
