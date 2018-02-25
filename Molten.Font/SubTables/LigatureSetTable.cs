using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class LigatureSetTable : FontSubTable
    {
        /// <summary>
        /// Gets an array of <see cref="LigatureTable"/>, ordered by preference.
        /// </summary>
        public LigatureTable[] Tables { get; internal set; }

        internal LigatureSetTable(BinaryEndianAgnosticReader reader, Logger log, IFontTable parent, long offset) : 
            base(reader, log, parent, offset)
        {
            ushort ligatureCount = reader.ReadUInt16();
            ushort[] ligatureOffsets = reader.ReadArrayUInt16(ligatureCount);
            Tables = new LigatureTable[ligatureCount];
            for (int i = 0; i < ligatureCount; i++)
                Tables[i] = new LigatureTable(reader, log, this, ligatureOffsets[i]);
        }
    }

    public class LigatureTable : FontSubTable
    {
        /// <summary>
        /// Gets the glyph ID of the ligature to substitute.
        /// </summary>
        public ushort LigatureGlyph { get; private set; }

        /// <summary>
        /// Gets an array of component glyph IDs — starting with the second component, ordered in writing direction.
        /// </summary>
        public ushort[] ComponentGlyphIDs { get; private set; }

        internal LigatureTable(BinaryEndianAgnosticReader reader, Logger log, IFontTable parent, long offset) : 
            base(reader, log, parent, offset)
        {
            LigatureGlyph = reader.ReadUInt16();
            ushort componentCount = reader.ReadUInt16();
            ComponentGlyphIDs = reader.ReadArrayUInt16(componentCount - 1);
        }
    }
}
