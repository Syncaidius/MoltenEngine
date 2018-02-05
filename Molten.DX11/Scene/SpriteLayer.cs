using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class SpriteLayer
    {
        public List<ISprite> Sprites = new List<ISprite>();

        public bool Visible = true;
    }
}
