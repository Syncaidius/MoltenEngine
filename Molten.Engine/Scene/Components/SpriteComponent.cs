using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class SpriteComponent : RenderableComponent<ISprite>
    {
        public override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);
            _renderable.Position = Object.Transform.GlobalPosition;
            _renderable.Rotation = Object.Transform.GlobalRotation;
        }
    }
}
