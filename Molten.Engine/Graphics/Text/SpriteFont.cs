using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Font;
using Newtonsoft.Json;

namespace Molten.Graphics
{
    public class SpriteFont
    {
        public event ObjectHandler<SpriteFont> OnSizeChanged;

        float _fontSize;

        SpriteFontBinding _binding;
        internal SpriteFont(SpriteFontManager manager, SpriteFontBinding binding, float size)
        {
            Manager = manager;
            _binding = binding;
            Size = size;
        }

        public float GetAdvanceWidth(char c)
        {
            SpriteFontGlyphBinding glyphBinding = _binding.GetCharacter(c);
            return glyphBinding.AdvanceWidth * Scale;
        }

        public float GetHeight(char c)
        {
            SpriteFontGlyphBinding glyphBinding = _binding.GetCharacter(c);
            return glyphBinding.AdvanceHeight * Scale;
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
                SpriteFontGlyphBinding glyphBinding = _binding.GetCharacter(text[i]);
                result.X += glyphBinding.AdvanceWidth * Scale;
                result.Y = Math.Max(result.Y, glyphBinding.AdvanceHeight);
            }

            result.Y *= Scale;

            return result;
        }

        public SpriteFontGlyphBinding GetCharacter(char c)
        {
            return _binding.GetCharacter(c);
        }

        public SpriteFontManager Manager { get; }


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

        /// <summary>
        /// Gets the <see cref="FontFile"/> from which the font was loaded.
        /// </summary>
        public FontFile File => _binding.File;

        [JsonProperty]
        public float Size
        {
            get => _fontSize;
            set
            {
                if (_fontSize != value)
                {
                    _fontSize = value;
                    Scale = (_fontSize / Manager.BaseFontSize);
                    LineSpacing = Manager.DesignToPixels(_binding.File, _binding.File.HorizonalHeader.LineSpace) * Scale;
                    OnSizeChanged?.Invoke(this);
                }
            }
        }
    }
}
