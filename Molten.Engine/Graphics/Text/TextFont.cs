using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;
using Newtonsoft.Json;

namespace Molten.Graphics
{
    public class TextFont
    {
        public event ObjectHandler<TextFont> OnSizeChanged;

        float _fontSize;

        public TextFont(TextFontSource source, float fontSize)
        {
            Source = source;
            Size = fontSize;
        }

        public float GetAdvanceWidth(char c)
        {
            TextFontSource.CachedGlyph cache = Source.GetCharGlyph(c);
            return cache.AdvanceWidth * Scale;
        }

        public float GetHeight(char c)
        {
            TextFontSource.CachedGlyph cache = Source.GetCharGlyph(c);
            return cache.AdvanceHeight * Scale;
        }

        /// <summary>Measures the provided string and returns it's width and height, in pixels.</summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Vector2F MeasureString(string text)
        {
            return MeasureString(text, 0, text.Length);
        }

        /// <summary>Measures part (or all) of the provided string based on the provided maximum length. Returns its width and height in pixels.</summary>
        /// <param name="text">The text.</param>
        /// <param name="maxLength">The maximum length of the string to measure.</param>
        /// <returns></returns>
        public Vector2F MeasureString(string text, int maxLength)
        {
            return MeasureString(text, 0, maxLength);
        }

        /// <summary>Measures part (or all) of the provided string and returns its width and height, in pixels.</summary>
        /// <param name="text">The text.</param>
        /// <param name="startIndex">The starting character index within the string from which to begin measuring.</param>
        /// <param name="length">The number of characters to measure from the start index.</param>
        /// <returns></returns>
        public Vector2F MeasureString(string text, int startIndex, int length)
        {
            Vector2F result = new Vector2F();
            int end = startIndex + Math.Min(text.Length, length);

            for (int i = startIndex; i < end; i++)
            {
                TextFontSource.CachedGlyph cache = Source.GetCharGlyph(text[i]);
                result.X += cache.AdvanceWidth * Scale;
                result.Y = Math.Max(result.Y, cache.AdvanceHeight);
            }

            result.Y *= Scale;

            return result;
        }

        /// <summary>
        /// Returns the index of the nearest character within the specified string, based on the provided local point position.
        /// </summary>
        /// <param name="text">A string of text.</param>
        /// <param name="localPoint">The local point to test, relative to the string's screen or world position.</param>
        /// <returns></returns>
        public int GetNearestCharacter(string text, Vector2F localPoint)
        {
            // TODO This is needed when hit-testing for text editing.
            throw new NotImplementedException();
        }

        [JsonProperty]
        public float Size
        {
            get => _fontSize;
            set
            {
                if(_fontSize != value)
                {
                    _fontSize = value;
                    Scale = (_fontSize / TextFontSource.BASE_FONT_SIZE);
                    LineSpacing = Source.ToPixels(Source.Font.HorizonalHeader.LineSpace) * Scale;
                    OnSizeChanged?.Invoke(this);
                }
            }
        }

        public TextFontSource Source { get; private set; }

        [JsonProperty]
        /// <summary>
        /// Gets the font's recommended line spacing between two lines, in pixels.
        /// </summary>
        public float LineSpacing { get; private set; }

        /// <summary>
        /// Gets the scale used when converting from <see cref="TextFontSource.BASE_FONT_SIZE"/> to the font size of the current <see cref="TextFont"/>.
        /// </summary>
        [JsonProperty]
        public float Scale { get; private set; }
    }
}
