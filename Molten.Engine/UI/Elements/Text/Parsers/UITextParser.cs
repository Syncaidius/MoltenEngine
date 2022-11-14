using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    /// <summary>
    /// Provides a base for custom text parsers. These can be used for anything from custom formatting to syntax highlighting.
    /// </summary>
    public abstract class UITextParser
    {
        /// <summary>
        /// Gets or sets a list of characters that are considered whitespace
        /// </summary>
        public char[] Whitespace { get; protected set; } = { ' ', '\t' };

        /// <summary>
        /// Gets or sets a list of characters that are considered punctuation.
        /// </summary>
        public char[] Punctuation { get; protected set; } = { '.', ',', ':', ';', '\'', '"' };

        public string[] NewLineCharacters { get; protected set; } =  { Environment.NewLine, "\r", "\n" };

        public abstract void ParseText(UITextElement element, string text);
    }

    public class UIDefaultTextParser : UITextParser
    {
        public override void ParseText(UITextElement element, string text)
        {
            if (!element.IsMultiLine)
            {
                for (int i = 0; i < NewLineCharacters.Length; i++)
                    text = text.Replace(NewLineCharacters[i], "");
            }
            else
            {
                string[] lines = Regex.Split(text, "\r?\n");
                for (int i = 0; i < lines.Length; i++)
                {
                    UITextLine line = new UITextLine(element);

                    for (int t = 0; t < text.Length; t++)
                    {
                        char c = text[t];

                        UITextSegmentType charType = ParseRuleCharList(c, seg, font, Parent.Rules.Whitespace, UITextSegmentType.Whitespace);

                        if (charType == UITextSegmentType.Text)
                            charType = ParseRuleCharList(c, seg, font, Parent.Rules.Punctuation, UITextSegmentType.Punctuation);

                        if (seg.Type != charType)
                            seg = InsertSegment(seg, Color.White, font, charType);

                        // TODO check rules for any other segmenting operators/symbols (e.g. brackets, math symbols, numbers/words, etc).
                        // TODO check if the current segment text is equal to any keywords

                        seg.Text += c;
                    }

                    element.AppendLine(line);
                }
            }
        }

        private UITextSegmentType ParseRuleCharList(UITextElement element, char c, UITextSegment seg, SpriteFont font, char[] list, UITextSegmentType type)
        {
            UITextSegmentType charType = UITextSegmentType.Text;

            // Check for whitespace character
            for (int w = 0; w < list.Length; w++)
            {
                // Start new segment
                if (c == list[w])
                {
                    if (seg.Type != type)
                    {
                        seg = InsertSegment(seg, Color.White, font, type);
                        seg.Type = type;
                    }

                    charType = type;
                    break;
                }
            }

            return charType;
        }
    }
}
