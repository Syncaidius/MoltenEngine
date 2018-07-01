using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    internal class SceneRemoveObject2D : SceneChange<SceneRemoveObject2D>
    {
        internal IRenderable2D Sprite;

        public override void Clear()
        {
            Sprite = null;
        }

        internal override void Process(Scene scene)
        {
            scene.Renderables2d.Remove(Sprite);
            scene.RenderData.RemoveObject(Sprite);

            // UI components are always IUpdatable.
            if (Sprite is IUpdatable up)
                scene.Updatables.Remove(up);

            Recycle(this);
        }
    }
}
