using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="SceneObject"/> to the root of a scene.</summary>
    internal class SpriteSetLayerCount : RenderSceneChange<SpriteSetLayerCount> 
    {
        public int LayerCount;

        public override void Clear() { }

        public override void Process(SceneRenderDataDX11 scene)
        {
            int oldSize = scene.SpriteLayers.Length;
            if (oldSize == LayerCount)
                return;

            Array.Resize(ref scene.SpriteLayers, LayerCount);
            for (int i = oldSize; i < scene.SpriteLayers.Length; i++)
                scene.SpriteLayers[i] = new SpriteLayer();

            Recycle(this);
        }
    }
}
