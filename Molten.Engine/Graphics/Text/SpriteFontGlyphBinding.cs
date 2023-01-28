using Molten.Font;

namespace Molten.Graphics
{
    public class SpriteFontGlyphBinding
    {
        public Glyph Glyph { get; init; }

        public Rectangle Location { get; internal set; }

        internal int PageID { get; set; }

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

        internal SpriteFontBinding Font { get; }

        internal SpriteFontGlyphBinding(SpriteFontBinding font)
        {
            Font = font;
        }
    }
}
