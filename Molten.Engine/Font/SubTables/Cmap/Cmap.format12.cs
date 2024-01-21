using Molten.IO;

namespace Molten.Font;

public class CmapFormat12SubTable : CmapSubTable
{
    uint _language;
    SequenceMapGroupRecord[] _records;

    internal CmapFormat12SubTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, CmapEncodingRecord record) :
        base(reader, log, parent, offset, record)
    {
        ushort reserved = reader.ReadUInt16();
        uint length = reader.ReadUInt32();
        _language = reader.ReadUInt32();
        uint numGroups = reader.ReadUInt32();

        _records = new SequenceMapGroupRecord[numGroups];
        for (int i = 0; i < numGroups; i++)
        {
            _records[i] = new SequenceMapGroupRecord()
            {
                StartCharCode = reader.ReadUInt32(),
                EndCharCode = reader.ReadUInt32(),
                StartGlyphID = reader.ReadUInt32(),
            };
        }
    }

    public override ushort CharPairToGlyphIndex(uint codepoint, ushort defaultGlyphIndex, uint nextCodepoint)
    {
        return 0;
    }

    public override ushort CharToGlyphIndex(uint codepoint)
    {
        // MS docs: You search for the first endCode that is greater than or equal to the character code you want to map. 
        int segID = 0;
        for (int i = 0; i < _records.Length; i++)
        {
            if (_records[i].EndCharCode >= codepoint)
            {
                segID = i;
                break;
            }
        }

        if (_records[segID].StartCharCode <= codepoint)
            return (ushort)(_records[segID].StartGlyphID + (codepoint - _records[segID].StartCharCode));
        else
            return 0;
    }

    private struct SequenceMapGroupRecord
    {
        public uint StartCharCode;

        public uint EndCharCode;

        public uint StartGlyphID;
    }
}
