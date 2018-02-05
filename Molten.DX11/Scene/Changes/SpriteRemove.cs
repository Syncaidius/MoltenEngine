using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="SceneObject"/> to the root of a scene.</summary>
    internal class SpriteRemove : RenderSceneChange<SpriteRemove> 
    {
        public ISprite Sprite;

        public int Layer;

        public override void Clear()
        {
            Sprite = null;
        }

        public override void Process(SceneRenderDataDX11 scene)
        {
            scene.SpriteLayers[Layer].Sprites.Remove(Sprite);
            Recycle(this);
        }
    }
}
