using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Font
{
    /// <summary>See: https://www.microsoft.com/typography/otspec/gdef.htm - "Glyph Class Definition Table"</summary>
    public enum GlyphClassDefinition : ushort
    {
        Zero = 0,

        /// <summary>Base glyph (single character, spacing glyph)</summary>
        Base = 1,

        /// <summary>Ligature glyph (multiple character, spacing glyph)</summary>
        Ligature = 2,

        /// <summary>Mark glyph (non-spacing combining glyph)</summary>
        Mark = 3,

        /// <summary>Component glyph (part of single character, spacing glyph)</summary>
        Component = 4,
    }
}
