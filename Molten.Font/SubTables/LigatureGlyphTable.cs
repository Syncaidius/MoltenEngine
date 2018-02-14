using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class LigatureGlyphTable
    {
        /// <summary>Gets the byte offset of the <see cref="LigatureGlyphTable"/> within it's parent <see cref="LigatureCaretListTable"/>.</summary>
        public ushort Offset { get; private set; }

        /// <summary>Gets an array of <see cref="CaretValue"/> for a glyph.</summary>
        public CaretValue[] CaretValues { get; internal set; }

        /// <summary>Gets the parent <see cref="LigatureCaretListTable"/> of the current <see cref="LigatureGlyphTable"/>.</summary>
        public LigatureCaretListTable Parent { get; private set; }

        /// <summary>Gets the ID of the glyph that the <see cref="CaretValues"/> correspond to.</summary>
        public ushort GlyphID { get; internal set; }

        internal LigatureGlyphTable(LigatureCaretListTable parent, ushort offset)
        {
            Parent = parent;
            Offset = offset;
        }
    }
}
