using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    internal class SceneRemoveSprite : SceneChange<SceneRemoveSprite>
    {
        public ISprite Sprite;

        public override void Clear()
        {
            Sprite = null;
        }

        public override void Process(Scene scene)
        {
            scene.Sprites.Remove(Sprite);
            if (Sprite is IUpdatable up)
                scene.Updatables.Remove(up);
            Recycle(this);
        }
    }
}
