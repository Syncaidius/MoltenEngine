using Molten.Graphics;
using Molten.UI;
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
            scene.RenderData.AddSprite(Sprite);

            // UI components are always IUpdatable, which we don't actually want in the scene's updatables list.
            // The scene's UI system will update it's root components at the correct time.
            if (Sprite is UIComponent com)
                scene.UI.AddUI(com);
            else if (Sprite is IUpdatable up)
                scene.Updatables.Add(up);

            Recycle(this);
        }
    }
}
