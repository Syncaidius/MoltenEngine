using Molten.Graphics;
using System.Text.RegularExpressions;

namespace Molten.UI;

/// <summary>
/// Provides a base for custom text parsers. These can be used for anything from custom formatting to syntax highlighting.
/// </summary>
public abstract class UITextParser
{
    /// <summary>
    /// Invoked when the parser should populate the provided <see cref="UITextBox"/>.
    /// </summary>
    /// <param name="element">The <see cref="UITextBox"/> to be populated with parsed text.</param>
    /// <param name="text">The text to be parsed.</param>
    internal void ParseText(UITextBox element, string text)
    {
        OnParseText(element, text);
    }

    protected abstract void OnParseText(UITextBox element, string text);
}

public class UIDefaultTextParser : UITextParser
{
    /// <summary>
    /// Gets or sets a list of characters that are considered whitespace
    /// </summary>
    static readonly char[] _whitespace = { ' ', '\t' };

    /// <summary>
    /// Gets or sets a list of characters that are considered punctuation.
    /// </summary>
    static readonly char[] _punctuation = { '.', ',', ':', ';', '\'', '"' };

    static readonly string[] _newLineChars = { Environment.NewLine, "\r", "\n" };

    /// <inheritdoc/>
    protected override void OnParseText(UITextBox element, string text)
    {
        string[] lines = Regex.Split(text, "\r?\n");

        for (int i = 0; i < lines.Length; i++)
        {
            ref string lineText = ref lines[i];

            UITextLine line = element.NewLine();
            string segText = "";

            for (int t = 0; t < lineText.Length; t++)
            {
                char c = lineText[t];
                UITextSegment segSeparate = ParseRuleCharList(element, c, element.DefaultFont, _whitespace);

                if (segSeparate == null)
                    segSeparate = ParseRuleCharList(element, c, element.DefaultFont, _punctuation);

                if (segSeparate != null)
                {
                    UITextSegment seg = new UITextSegment(segText, Color.White, element.DefaultFont);
                    line.AppendSegment(seg);
                    line.AppendSegment(segSeparate);
                    segText = "";
                }
                else
                {
                    segText += c;
                }
            }

            // Append the remaining text.
            if(segText.Length > 0)
            {
                UITextSegment seg = new UITextSegment(segText, Color.White, element.DefaultFont);
                line.AppendSegment(seg);
            }
        }

        element.Recalculate();
    }

    private UITextSegment ParseRuleCharList(UITextBox element, char c, SpriteFont font, char[] list)
    {
        // Check for whitespace character
        for (int w = 0; w < list.Length; w++)
        {
            // Start new segment
            if (c == list[w])
                return new UITextSegment(c.ToString(), Color.White, font);
        }

        return null;
    }
}
