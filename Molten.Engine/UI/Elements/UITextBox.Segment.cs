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

            public string Text = "";

            public Segment Previous;

            public Segment Next;

            public Color Color = Color.White;

            public RectangleF Bounds;

            public SegmentType Type;

            public Segment() { }

            public Segment(string text, Color color, SpriteFont font, SegmentType type)
            {
                Text = text;
                Color = color;
                Font = font;
                Type = type;
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

        public enum SegmentType
        {
            Text = 0,

            Whitespace = 1,

            Punctuation = 2,
        }
    }
}
