using Molten.IO;

namespace Molten.Font
{
    public class SequenceTable : FontSubTable
    {
        /// <summary>
        /// Gets an array of glyph IDs.
        /// </summary>
        public ushort[] GlyphIDs { get; internal set; }

        internal SequenceTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
            base(reader, log, parent, offset)
        {
            ushort glyphCount = reader.ReadUInt16();
            GlyphIDs = reader.ReadArray<ushort>(glyphCount);
        }
    }
}
