using System.Runtime.CompilerServices;
using Molten.Font;
using Newtonsoft.Json;

namespace Molten.Graphics
{
    public class SpriteFont
    {
        /// <summary>
        /// Invoked when <see cref="Size"/> is changed.
        /// </summary>
        public event ObjectHandler<SpriteFont> OnSizeChanged;

        float _fontSize;
        SpriteFontBinding _binding;

        internal SpriteFont(SpriteFontManager manager, SpriteFontBinding binding, float size)
        {
            Manager = manager;
            _binding = binding;
            Size = size;
        }

        /// <summary>
        /// Gets the advance width of the specified character, in pixels.
        /// </summary>
        /// <param name="c">The character.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetAdvanceWidth(char c)
        {
            SpriteFontGlyphBinding glyphBinding = _binding.GetCharacter(c);
            return glyphBinding.AdvanceWidth * Scale;
        }

        /// <summary>
        /// Gets the height of the specified character, in pixels.
        /// </summary>
        /// <param name="c">The character.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetHeight(char c)
        {
            SpriteFontGlyphBinding glyphBinding = _binding.GetCharacter(c);
            return glyphBinding.AdvanceHeight * Scale;
        }

        /// <summary>Measures the provided string and returns it's width and height, in pixels.</summary>
        /// <param name="text">The string of text to be measured.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2F MeasureString(string text)
        {
            return MeasureString(text, 0, text.Length);
        }

        /// <summary>Measures part (or all) of the provided string based on the provided maximum length. Returns its width and height in pixels.</summary>
        /// <param name="text">The string of text to be measured.</param>
        /// <param name="maxLength">The maximum length of the string to measure.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2F MeasureString(string text, int maxLength)
        {
            return MeasureString(text, 0, maxLength);
        }

        /// <summary>Measures part (or all) of the provided string and returns its width and height, in pixels.</summary>
        /// <param name="text">The string of text to be measured.</param>
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
                result.Y = float.Max(result.Y, glyphBinding.AdvanceHeight);
            }

            result.Y *= Scale;

            return result;
        }

        /// <summary>
        /// Measures the width and height of a single character in pixels, using the current <see cref="SpriteFont"/>.
        /// </summary>
        /// <param name="c">The character to measure.</param>
        /// <returns></returns>
        public Vector2F MeasureChar(char c)
        {
            SpriteFontGlyphBinding glyphBinding = _binding.GetCharacter(c);
            return new Vector2F(
                glyphBinding.AdvanceWidth * Scale,
                glyphBinding.AdvanceHeight * Scale
            );
        }

        /// <summary>
        /// Measures the width of a single character in pixels, using the current <see cref="SpriteFont"/>.
        /// </summary>
        /// <param name="c">The character to measure.</param>
        /// <returns></returns>
        public float MeasureCharWidth(char c)
        {
            SpriteFontGlyphBinding glyphBinding = _binding.GetCharacter(c);
            return glyphBinding.AdvanceWidth * Scale;
        }

        /// <summary>Measures part (or all) of the provided string and returns its width, in pixels.</summary>
        /// <param name="text">The string of text to be measured.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float MeasureWidth(string text)
        {
            return MeasureWidth(text, 0, text.Length);
        }

        /// <summary>Measures part (or all) of the provided string and returns its width, in pixels.</summary>
        /// <param name="text">The string of text to be measured.</param>
        /// <param name="maxLength">The maximum length of the string to measure.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float MeasureWidth(string text, int maxLength)
        {
            return MeasureWidth(text, 0, maxLength);
        }

        /// <summary>Measures part (or all) of the provided string and returns its width, in pixels.</summary>
        /// <param name="text">The string of text to be measured.</param>
        /// <param name="startIndex">The starting character index within the string from which to begin measuring.</param>
        /// <param name="length">The number of characters to measure from the start index.</param>
        /// <returns></returns>
        public float MeasureWidth(string text, int startIndex, int length)
        {
            float result = 0;
            int end = startIndex + Math.Min(text.Length, length);

            for (int i = startIndex; i < end; i++)
                result += GetAdvanceWidth(text[i]);

            return result;
        }

        /// <summary>
        /// Retrieves a <see cref="SpriteFontGlyphBinding"/> for the specified character.
        /// </summary>
        /// <param name="c">The character.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpriteFontGlyphBinding GetCharacter(char c)
        {
            return _binding.GetCharacter(c);
        }

        /// <summary>
        /// Gets the parent <see cref="SpriteFontManager"/> which instantiated the current <see cref="SpriteFont"/>.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the size of the current <see cref="SpriteFont"/>.
        /// </summary>
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
