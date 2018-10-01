using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class Sprite
    {
        public Rectangle Source { get; set; }

        public float ArraySlice { get; set; }

        public ITexture2D Texture { get; set; }

        public Vector2F Position { get; set; }

        public float Rotation { get; set; }

        public Vector2F Scale { get; set; }

        public Vector2F Origin { get; set; }

        public IMaterial Material { get; set; }

        public Color Color { get; set; }

        public float Depth { get; set; }
    }
}
