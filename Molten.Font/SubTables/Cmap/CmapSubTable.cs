using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public abstract class CmapSubTable : FontSubTable
    {
        public ushort Format { get; private set; }

        public FontPlatform Platform { get; protected set; }

        public ushort Encoding { get; protected set; }

        public ushort Language { get; protected set; }

        public abstract ushort CharToGlyphIndex(uint codepoint);

        public abstract ushort CharPairToGlyphIndex(uint codepoint, ushort defaultGlyphIndex, uint nextCodepoint);

        internal CmapSubTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, CmapEncodingRecord record) :
            base(reader, log, parent, offset)
        {
            Format = reader.ReadUInt16(); // We're reading the format again because the stream was moved back to the start of the table.
            Platform = record.Platform;
            Encoding = record.Encoding;
        }
    }
}
