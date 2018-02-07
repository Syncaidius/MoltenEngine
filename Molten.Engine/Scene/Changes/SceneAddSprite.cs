using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    internal class SceneAddSprite : SceneChange<SceneAddSprite>
    {
        public ISprite Sprite;

        public int Layer;

        public override void Clear()
        {
            Sprite = null;
        }

        public override void Process(Scene scene)
        {
            scene.Sprites.Add(Sprite);
            scene.RenderData.AddSprite(Sprite, Layer);
            if (Sprite is IUpdatable up)
                scene.Updatables.Add(up);
            Recycle(this);
        }
    }
}
