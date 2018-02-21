using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    [Flags]
    /// <summary>Determines available glyph/outline types of an OpenType/TrueType font.</summary>
    public enum FontFlags : uint
    {
        Invalid = 0,

        Valid = 1,

        /// <summary>The font contains everything needed for using TrueType glyphs/outlines.</summary>
        TrueType = 1 << 1,

        /// <summary>The font contains everything needed for using CFF glyphs/outlines.</summary>
        CFF = 1 << 2,

        /// <summary>The font contains everything needed for using CFF 2.0 glyphs/outlines.</summary>
        CFF2 = 1 << 3,

        /// <summary>
        /// The font contains SVG glyph data (generally used for colored/styled glyphs (e.g. emojis).
        /// </summary>
        SVG = 1 << 4,

        /// <summary>
        /// The font contains bitmaps of glyphs in addition to outlines. <para/>
        /// Hand-tuned bitmaps are especially useful in OpenType fonts for representing complex glyphs at very small sizes. <para/>
        /// If a bitmap for a particular size is provided in a font, it may be used by the system instead of the outline when rendering the glyph. 
        /// </summary>
        Bitmaps = 1 << 5,

        /// <summary>The font contains a digital signature. This is generally true if a <see cref="DSIG"/> table is present in the font.</summary>
        DigitallySigned = 1 << 6,

        /// <summary>
        /// The font contains kerning data. This data can be used to control the inter-character spacing for the glyphs in a font. <para/>
        /// </summary>
        Kerning = 1 << 7,
    }
}
