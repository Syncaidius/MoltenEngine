using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Font;

namespace Molten.Graphics
{
    internal class FontGlyphBinding
    {
        internal Glyph Glyph { get; }

        internal FontFile Font { get; }

        internal Rectangle Location { get; set; }

        internal int Page { get; set; }

        /// <summary> The advance width (horizontal advance) of the character glyph, in pixels. </summary>
        public readonly int AdvanceWidth;

        /// <summary>
        /// The advance height (vertical advance) of the character glyph, in pixels.
        /// </summary>
        public readonly int AdvanceHeight;

        /// <summary>The number of pixels along the Y-axis that the glyph was offset, before fitting on to the font atlas. </summary>
        public readonly float YOffset;

        public readonly int PWidth;

        public readonly int PHeight;

        internal FontGlyphBinding(FontManager manager, FontFile font, char c)
        {
            Font = font;

            ushort gIndex = font.GetGlyphIndex(c);
            Glyph = font.GetGlyphByIndex(gIndex);
            GlyphMetrics gm = font.GetMetricsByIndex(gIndex);

            Rectangle gBounds = Glyph.Bounds;
            gBounds.Width = Math.Max(1, gBounds.Width);
            gBounds.Height = Math.Max(1, gBounds.Height);

            AdvanceWidth = manager.ToPixels(font, gm.AdvanceWidth);
            AdvanceHeight = manager.ToPixels(font, font.Header.MaxY);
            PWidth = manager.ToPixels(font, gBounds.Width);
            PHeight = manager.ToPixels(font, gBounds.Height); 
            YOffset = manager.ToPixels(font, gBounds.Top);
        }
    }
}
