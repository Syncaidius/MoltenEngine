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

        public abstract ushort CharToGlyphIndex(int codepoint);

        public abstract ushort CharPairToGlyphIndex(int codepoint, ushort defaultGlyphIndex, int nextCodepoint);

        internal CmapSubTable(BinaryEndianAgnosticReader reader, Logger log, IFontTable parent, long offset, CmapEncodingRecord record) :
            base(reader, log, parent, offset)
        {
            Format = record.Format;
            Platform = record.Platform;
            Encoding = record.Encoding;
        }
    }
}
