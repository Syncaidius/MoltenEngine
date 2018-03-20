using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class SequenceTable : FontSubTable
    {
        /// <summary>
        /// Gets an array of glyph IDs.
        /// </summary>
        public ushort[] GlyphIDs { get; internal set; }

        internal override void Read(EnhancedBinaryReader reader, FontReaderContext context, FontTable parent)
        {
            ushort glyphCount = reader.ReadUInt16();
            GlyphIDs = reader.ReadArray<ushort>(glyphCount);
        }
    }
}
