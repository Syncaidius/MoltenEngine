using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>
    /// Indicates a relative change from the normal aspect ratio (width to height ratio) as specified by a font designer for the glyphs in a font.
    /// </summary>
    public enum FontWidthClass : ushort
    {
        None = 0,

        UltraCondensed = 1,

        ExtraCondensed = 2,

        Condensed = 3,

        SemiCondensed = 4,

        Medium = 5,

        SemiExpanded = 6,

        Expanded = 7,

        ExtraExpanded = 8,

        UltraExpanded = 9,
    }
}
