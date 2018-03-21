using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class CmapFormat0SubTable : CmapSubTable
    {
        byte[] _glyphIDs;

        internal CmapFormat0SubTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, CmapEncodingRecord record) : 
            base(reader, log, parent, offset, record)
        {
            /* This is a simple 1 to 1 mapping of character codes to glyph indices.
                * The glyph set is limited to 256. Note that if this format is used to index into a larger glyph set, only the first 256 glyphs will be accessible.*/

            Header.Length = reader.ReadUInt16();
            Language = reader.ReadUInt16();

            // Faster to read all bytes then re-iterate them in to a ushort array, 
            // compared to reading 1 byte at a time from the stream.
            _glyphIDs = reader.ReadBytes(256);
        }

        public override ushort CharPairToGlyphIndex(int codepoint, ushort defaultGlyphIndex, int nextCodepoint)
        {
            return 0;
        }

        public override ushort CharToGlyphIndex(int codepoint)
        {
            // Only 8-bit lookups are supported in this format.
            if (codepoint < 256)
                return _glyphIDs[codepoint];
            else
                return 0;
        }
    }
}
