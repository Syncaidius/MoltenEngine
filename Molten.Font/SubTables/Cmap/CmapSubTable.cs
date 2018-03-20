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

        public CmapEncodingRecord EncodingRecord { get; internal set; }

        public ushort Language { get; protected set; }

        public abstract ushort CharToGlyphIndex(int codepoint);

        public abstract ushort CharPairToGlyphIndex(int codepoint, ushort defaultGlyphIndex, int nextCodepoint);
    }
}
