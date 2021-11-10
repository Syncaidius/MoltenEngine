namespace Molten.Font
{
    /// <summary>
    /// A table which always returns the default character. Intended as a placeholder for unsupported cmap sub-table formats.
    /// </summary>
    public class CmapNullSubTable : CmapSubTable
    {
        byte[] _glyphIDs;

        internal CmapNullSubTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, CmapEncodingRecord record) :
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

        public override ushort CharPairToGlyphIndex(uint codepoint, ushort defaultGlyphIndex, uint nextCodepoint)
        {
            return 0;
        }

        public override ushort CharToGlyphIndex(uint codepoint)
        {
            return 0;
        }
    }
}
