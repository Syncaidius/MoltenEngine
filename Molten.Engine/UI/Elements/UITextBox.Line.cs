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
            internal Line(UITextBox textbox)
            {
                TextBox = textbox;
            }

            public void SetText(SpriteFont font, string text)
            {
                Width = 0;
                Height = 0;

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
                    // TODO check if the current segment text is equal to any keywords

                    seg.Text += c;
                }

                // Set width of last node, then add it to total width and height.
                seg.Size = font.MeasureString(seg.Text);
                Width += seg.Size.X;
                Height = Math.Max(Height, (int)Math.Ceiling(seg.Size.Y));
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

                seg.Size = font.MeasureString(seg.Text);
                seg.Next = next;
                next.Previous = seg;

                Width += seg.Size.X;
                Height = Math.Max(Height, (int)Math.Ceiling(seg.Size.Y));

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

            public float Width { get; private set; }

            public int Height { get; private set; }

            public UITextBox TextBox { get; internal set; }

            public Segment First { get; private set; }
        }
    }
}
