using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class CmapFormat4SubTable : CmapSubTable
    {
        ushort[] _startCode;
        ushort[] _endCode;    
        short[] _idDelta;
        ushort[] _idRangeOffset;
        ushort[] _glyphIdArray;

        internal CmapFormat4SubTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, CmapEncodingRecord record) : 
            base(reader, log, parent, offset, record)
        {
            Header.Length = reader.ReadUInt16() - 2U; // Subtract 2 because it also includes the length value in the byte size...
            Language = reader.ReadUInt16();

            ushort segCountX2 = reader.ReadUInt16();
            ushort searchRange = reader.ReadUInt16();
            ushort entrySelector = reader.ReadUInt16();
            ushort rangeShift = reader.ReadUInt16();
            int segCount = segCountX2 / 2;

            _endCode = reader.ReadArrayUInt16(segCount);
            ushort reservedPad = reader.ReadUInt16();
            _startCode = reader.ReadArrayUInt16(segCount);
            _idDelta = reader.ReadArrayInt16(segCount);

            // Pre-modulo all the deltas to avoid performing a modulo calculation every time a character is looked up.
            for (int i = 0; i < _idDelta.Length; i++)
                _idDelta[i] = (short)(_idDelta[i] % 65536);

            _idRangeOffset = reader.ReadArrayUInt16(segCount);

            long tableEndPos = Header.ReadOffset + Header.Length;
            int numGlyphIDs = (int)(tableEndPos - reader.Position) / sizeof(ushort);
            _glyphIdArray = reader.ReadArrayUInt16(numGlyphIDs);
        }

        public override ushort CharPairToGlyphIndex(int codepoint, ushort defaultGlyphIndex, int nextCodepoint)
        {
            return 0;
        }

        public override ushort CharToGlyphIndex(int codepoint)
        {
            // Only 16-bit (or lower) lookups are supported in this format.
            if (codepoint > ushort.MaxValue)
                return 0;

            // MS docs: You search for the first endCode that is greater than or equal to the character code you want to map. 
            int segID = 0;
            for(int i = 0; i < _endCode.Length; i++)
            {
                if(_endCode[i] >= codepoint)
                {
                    segID = i;
                    break;
                }
            }

            if(_startCode[segID] <= codepoint)
            {
                /* MS docs: If the idRangeOffset is 0, the idDelta value is added directly to the character code offset (i.e. idDelta[i] + c) to get the corresponding glyph index. 
                 * Again, the idDelta arithmetic is modulo 65536. */
                if (_idRangeOffset[segID] == 0)
                    return (ushort)(_idDelta[segID] + codepoint);
                else
                {
                    /* MS docs: The character code offset from startCode is added to the idRangeOffset value. 
                     * This sum is used as an offset from the current location within idRangeOffset itself to index out the correct glyphIdArray value. 
                     * This obscure indexing trick works because glyphIdArray immediately follows idRangeOffset in the font file.*/
                    int offset = (_idRangeOffset[segID] / 2) + (codepoint - _startCode[segID]);
                    return _glyphIdArray[offset - _idRangeOffset.Length + segID];
                }
            }
            else
            {
                return 0;
            }
        }
    }
}
