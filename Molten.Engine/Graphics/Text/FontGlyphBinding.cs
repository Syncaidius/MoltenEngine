using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Font;

namespace Molten.Graphics
{
    public class FontGlyphBinding
    {
        public Glyph Glyph { get; init; }

        public Rectangle Location { get; internal set; }

        internal int Page { get; set; }

        /// <summary> The advance width (horizontal advance) of the character glyph, in pixels. </summary>
        public int AdvanceWidth { get; init; }

        /// <summary>
        /// The advance height (vertical advance) of the character glyph, in pixels.
        /// </summary>
        public int AdvanceHeight { get; init; }

        /// <summary>The number of pixels along the Y-axis that the glyph was offset, before fitting on to the font atlas. </summary>
        public float YOffset { get; init; }

        public int PWidth { get; init; }

        public int PHeight { get; init; }

        internal FontBinding Font { get; }

        internal FontGlyphBinding(FontBinding font)
        {
            Font = font;
        }
    }
}
