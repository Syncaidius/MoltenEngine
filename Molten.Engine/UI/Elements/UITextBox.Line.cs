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
            int _height;

            internal Line(UITextBox textbox)
            {
                Parent = textbox;
            }

            public void SetText(SpriteFont font, string text)
            {
                // TODO reuse Segment instances from a pool;

                Width = 0;
                _height = 0;
                HasText = true;
                First = new Segment("", Color.White, font, SegmentType.Text); 

                if(string.IsNullOrWhiteSpace(text))
                {
                    HasText = false;
                    return;
                }    

                Segment seg = First;

                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];

                    SegmentType charType = ParseRuleCharList(c, seg, font, Parent.Rules.Whitespace, SegmentType.Whitespace);

                    if(charType == SegmentType.Text)
                        charType = ParseRuleCharList(c, seg, font, Parent.Rules.Punctuation, SegmentType.Punctuation);

                    if (seg.Type != charType)
                        seg = AddNextNode(seg, Color.White, font, charType);

                    // TODO check rules for any other segmenting operators/symbols (e.g. brackets, math symbols, numbers/words, etc).
                    // TODO check if the current segment text is equal to any keywords

                    seg.Text += c;
                }

                // Set width of last node, then add it to total width and height.
                seg.Size = font.MeasureString(seg.Text);
                Width += seg.Size.X;
                _height = Math.Max(_height, (int)Math.Ceiling(seg.Size.Y));
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
                _height = Math.Max(_height, (int)Math.Ceiling(seg.Size.Y));

                return next;
            }

            public float Width { get; private set; }

            public int Height => HasText ? _height : Parent.DefaultLineHeight;

            public UITextBox Parent { get; internal set; }

            public Segment First { get; private set; }

            public bool HasText { get; private set; }
        }
    }
}
