using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;
using static Molten.UI.UITextBox;

namespace Molten.UI
{
    public partial class UITextBox
    {
        internal class Line
        {
            public uint LineNumber;

            public Vector2F LineNumberSize;

            public RectangleF SelectorBounds;

            RectangleF _textBounds;

            public void SetText(SpriteFont font, string text)
            {
                Vector2F size = font.MeasureString(text);

                First = new Segment()
                {
                    Text = text,
                    Color = Color.White,
                    Font = font,
                    Bounds = new RectangleF(Vector2F.Zero, size),
                };

                _textBounds.Width = size.X;
                _textBounds.Height = size.Y;
            }

            private void UpdatePosition()
            {
                if (First == null)
                    return;

                Segment seg = First;
                Vector2F p = _textBounds.TopLeft;

                while (seg != null)
                {
                    seg.Bounds.X = p.X;
                    seg.Bounds.Y = p.Y;

                    p.X += seg.Bounds.Width;
                    seg = seg.Next;
                }
            }

            public bool Contains(Vector2F pos)
            {
                return _textBounds.Contains(pos);
            }

            public Segment First { get; private set; }

            public RectangleF TextBounds => _textBounds;

            public Vector2F Position
            {
                get => _textBounds.TopLeft;
                set
                {
                    _textBounds.X = value.X;
                    _textBounds.Y = value.Y;
                    UpdatePosition();
                }
            }
        }

    }
}
