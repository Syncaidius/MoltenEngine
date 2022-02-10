using Molten.IO;

namespace Molten.Font
{
    public class CmapFormat6SubTable : CmapSubTable
    {
        ushort _startCode;
        ushort _endCode;
        ushort[] _glyphIdArray;

        internal CmapFormat6SubTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, CmapEncodingRecord record) :
            base(reader, log, parent, offset, record)
        {
            ushort length = reader.ReadUInt16();

            Language = reader.ReadUInt16();
            _startCode = reader.ReadUInt16();
            ushort entryCount = reader.ReadUInt16();
            _endCode = (ushort)(_startCode + (entryCount - 1));
            _glyphIdArray = reader.ReadArray<ushort>(entryCount);
        }

        public override ushort CharPairToGlyphIndex(uint codepoint, ushort defaultGlyphIndex, uint nextCodepoint)
        {
            return 0;
        }

        public override ushort CharToGlyphIndex(uint codepoint)
        {
            /* The firstCode and entryCount values specify a subrange (beginning at firstCode,length = entryCount) within the range of possible character codes. 
             * Codes outside of this subrange are mapped to glyph index 0. 
             * The offset of the code (from the first code) within this subrange is used as index to the glyphIdArray, which provides the glyph index value.*/
            if (codepoint >= _startCode && codepoint <= _endCode)
                return _glyphIdArray[codepoint - _startCode];
            else
                return 0;
        }
    }
}
