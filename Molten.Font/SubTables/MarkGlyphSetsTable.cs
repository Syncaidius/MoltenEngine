using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>A mark glyph set table. <para />
    /// Mark glyph sets are used in GSUB and GPOS lookups to filter which marks in a string are considered or ignored <para />
    /// See: https://www.microsoft.com/typography/otspec/gdef.htm</summary>
    public class MarkGlyphSetsTable : FontSubTable
    {
        public ushort Format { get; private set; }

        /// <summary>Gets an array of coverage tables. Each table represents a set of mark glyphs. <para />
        /// Mark glyph sets are used in GSUB and GPOS lookups to filter which marks in a string are considered or ignored</summary>
        public CoverageTable[] Sets {get; private set;}

        internal override void Read(EnhancedBinaryReader reader, FontReaderContext context, FontTable parent)
        {
            Format = reader.ReadUInt16();
            ushort setCount = reader.ReadUInt16();

            uint[] offsets = reader.ReadArray<uint>(setCount);
            Sets = new CoverageTable[setCount];

            // Populate coverage tables
            for (int i = 0; i < setCount; i++)
                Sets[i] = context.ReadSubTable<CoverageTable>(offsets[i]);
        }        
    }
}
