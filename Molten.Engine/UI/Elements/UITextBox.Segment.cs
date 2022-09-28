using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    public partial class UITextBox
    {
        public class Segment
        {
            public SpriteFont Font;

            public string Text;

            public Segment Previous;

            public Segment Next;

            public Color Color = Color.White;

            public RectangleF Bounds;

            public Segment() { }

            public Segment(string text, Color color, SpriteFont font)
            {
                Text = text;
                Color = color;
                Font = font;
                Vector2F size = font.MeasureString(text);
                Bounds.X = size.X;
                Bounds.Y = size.Y;
            }

            public virtual void Render(SpriteBatcher sb)
            {
                if (string.IsNullOrWhiteSpace(Text) || Color.A == 0)
                    return;

                sb.DrawString(Font, Text, Bounds.TopLeft, Color, null, 0);
            }
        }
    }
}
