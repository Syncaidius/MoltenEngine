using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class Sprite : IRenderable2D
    {
        public ITexture2D Texture;
        public Rectangle Source;
        public Vector2F Position;
        public float Rotation;
        public Vector2F Origin;
        public Color Color = Color.White;
        public Vector2F Scale = Vector2F.One;

        public void Render(SpriteBatch batch)
        {
            batch.Draw(Texture, Position, Source, Color, Rotation, Scale, Origin);
        }
    }
}
