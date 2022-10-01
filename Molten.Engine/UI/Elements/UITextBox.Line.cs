using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

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

            internal Line(UITextBox textbox)
            {
                TextBox = textbox;
            }

            public void SetText(SpriteFont font, string text)
            {
                _textBounds.Width = 0;
                _textBounds.Height = 0;

                First = new Segment("", Color.White, font, SegmentType.Text);

                Segment seg = First;

                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];

                    SegmentType charType = ParseRuleCharList(c, seg, font, TextBox.Rules.Whitespace, SegmentType.Whitespace);

                    if(charType == SegmentType.Text)
                        charType = ParseRuleCharList(c, seg, font, TextBox.Rules.Punctuation, SegmentType.Punctuation);

                    if (seg.Type != charType)
                        seg = AddNextNode(seg, Color.White, font, charType);
                    // TODO check rules for any other segmenting operators/symbols (e.g. brackets, math symbols, numbers/words, etc).

                    seg.Text += c;
                }

                // Set width of last node
                seg.Bounds = new RectangleF(Vector2F.Zero, font.MeasureString(seg.Text));

                _textBounds.Width += seg.Bounds.Width;
                _textBounds.Height = Math.Max(_textBounds.Height, seg.Bounds.Height);

                UpdatePosition();
            }

            private SegmentType ParseRuleCharList(char c, Segment seg, SpriteFont font, char[] list, SegmentType type)
            {
                SegmentType charType = SegmentType.Text;

                // Check for whitespace character
                for (int w = 0; w < list.Length; w++)
                {
                    // Start new segment
                    if (c == list[w])
                    {
                        if (seg.Type != type)
                        {
                            seg = AddNextNode(seg, Color.White, font, type);
                            seg.Type = type;
                        }

                        charType = type;
                        break;
                    }
                }

                return charType;
            }

            private Segment AddNextNode(Segment seg, Color color, SpriteFont font, SegmentType type)
            {
                Segment next = new Segment("", Color.White, font, type);

                seg.Bounds = new RectangleF(Vector2F.Zero, font.MeasureString(seg.Text));
                seg.Next = next;
                next.Previous = seg;

                _textBounds.Width += seg.Bounds.Width;
                _textBounds.Height = Math.Max(_textBounds.Height, seg.Bounds.Height);

                return next;
            }

            public Segment OnPressed(Vector2F position)
            {
                Segment seg = First;
                while(seg != null)
                {
                    if (seg.Bounds.Contains(position))
                        return seg;

                    seg = seg.Next;
                }

                return null;
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

            public UITextBox TextBox { get; internal set; }

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
