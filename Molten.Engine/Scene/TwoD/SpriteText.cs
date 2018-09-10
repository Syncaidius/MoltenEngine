using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class SpriteText : IRenderable2D, ISceneObject
    {
        public SpriteFont Font;
        public Vector2F Position;
        public Color Color = Color.White;
        public string Text = "";

        Scene ISceneObject.Scene { get; set; }

        SceneLayer ISceneObject.Layer { get; set; }

        public void Render(SpriteBatch batch)
        {
            batch.DrawString(Font, Text, Position, Color);
        }
    }
}
