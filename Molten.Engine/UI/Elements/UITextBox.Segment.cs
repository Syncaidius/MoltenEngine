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
            /// <summary>
            /// The font of the current <see cref="Segment"/>. If null, the parent <see cref="UITextBox.DefaultFont"/> will be used.
            /// </summary>
            public SpriteFont Font;

            public string Text = "";

            public Segment Previous;

            public Segment Next;

            public Color Color = Color.White;

            public Vector2F Size;

            public SegmentType Type;

            public Segment() { }

            public Segment(string text, Color color, SpriteFont font, SegmentType type)
            {
                Text = text;
                Color = color;
                Font = font;
                Type = type;
                Size = font.MeasureString(text);
            }

            public virtual void Render(SpriteBatcher sb, UITextBox owner, ref RectangleF bounds)
            {
                if (string.IsNullOrWhiteSpace(Text) || Color.A == 0)
                    return;

                sb.DrawString(Font ?? owner.DefaultFont, Text, bounds.TopLeft, Color, null, 0);
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
