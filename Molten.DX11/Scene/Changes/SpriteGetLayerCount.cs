using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="SceneObject"/> to the root of a scene.</summary>
    internal class SpriteGetLayerCount : RenderSceneChange<SpriteGetLayerCount> 
    {
        public Action<int> RetrievalCallback;

        public override void Clear()
        {
            RetrievalCallback = null;
        }

        public override void Process(SceneRenderDataDX11 scene)
        {
            RetrievalCallback?.Invoke(scene.SpriteLayers.Length);
            Recycle(this);
        }
    }
}
