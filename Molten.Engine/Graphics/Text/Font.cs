using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Font;
using Newtonsoft.Json;

namespace Molten.Graphics
{
    public class Font
    {
        FontBinding _binding;
        internal Font(FontManager manager, FontBinding binding)
        {
            Manager = manager;
            _binding = binding;
        }

        public FontGlyphBinding GetCharacter(char c)
        {
            return _binding.GetCharacter(c);
        }

        public FontManager Manager { get; }


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
