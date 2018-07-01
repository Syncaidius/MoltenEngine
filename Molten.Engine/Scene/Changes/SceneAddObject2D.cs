using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    internal class SceneAddObject2D : SceneChange<SceneAddObject2D>
    {
        public IRenderable2D Sprite;

        public override void Clear()
        {
            Sprite = null;
        }

        internal override void Process(Scene scene)
        {
            scene.Renderables2d.Add(Sprite);
            scene.RenderData.AddObject(Sprite);

            if (Sprite is IUpdatable up)
            {
                if(up.Scene != scene)
                    scene.Updatables.Add(up);
            }

            Recycle(this);
        }
    }
}
