using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class RectangleSprite : ISprite
    {
        public Rectangle Destination;
        public float Rotation;
        public Vector2F Origin;
        public Color Color = Color.White;

        public void Render(SpriteBatch batch)
        {
            batch.DrawRect(Destination, Color, Rotation, Origin);
        }
    }
}
