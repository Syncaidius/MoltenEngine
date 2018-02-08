using Molten.Graphics;
using Molten.UI;
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

        public int Layer;

        public override void Clear()
        {
            Sprite = null;
        }

        public override void Process(Scene scene)
        {
            scene.Sprites.Remove(Sprite);
            scene.RenderData.RemoveSprite(Sprite, Layer);

            // UI components are always IUpdatable.
            if (Sprite is UIComponent com)
                scene.UI.RemoveUI(com);
            else if (Sprite is IUpdatable up)
                scene.Updatables.Remove(up);

            Recycle(this);
        }
    }
}
