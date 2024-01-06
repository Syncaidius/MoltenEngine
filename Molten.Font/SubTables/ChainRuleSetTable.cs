using Molten.IO;

namespace Molten.Font;

public class ChainRuleSetTable : FontSubTable
{
    /// <summary>
    /// Gets an array of PosRule tables, ordered by preference.
    /// </summary>
    public ChainRuleTable[] Tables { get; internal set; }

    internal ChainRuleSetTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
        base(reader, log, parent, offset)
    {
        ushort posRuleCount = reader.ReadUInt16();
        ushort[] posRuleOffsets = reader.ReadArray<ushort>(posRuleCount);
        Tables = new ChainRuleTable[posRuleCount];

        for (int i = 0; i < posRuleCount; i++)
        {
            long fileOffset = Header.StreamOffset + posRuleOffsets[i];
            Tables[i] = new ChainRuleTable(reader, log, this, posRuleOffsets[i]);
        }
    }
}

/// <summary>
/// See for ChainPosRule table: https://docs.microsoft.com/en-us/typography/opentype/spec/gpos#chaining-context-positioning-format-2-class-based-glyph-contexts
/// See for ChainSubRule table: https://docs.microsoft.com/en-us/typography/opentype/spec/gsub#62-chaining-context-substitution-format-2-class-based-glyph-contexts
/// </summary>
public class ChainRuleTable : FontSubTable
{
    /// <summary>
    /// Gets an array of gylph or class IDs (depending on format) for a backtrack sequence.
    /// </summary>
    public ushort[] BacktrackSequence { get; internal set; }

    /// <summary>
    /// Gets an array of glyph or class IDs (depending on format) for an input sequence.
    /// </summary>
    public ushort[] InputSequence { get; internal set; }

    /// <summary>
    /// Gets an array of glyph or class IDs (depending on format) for a look ahead sequence.
    /// </summary>
    public ushort[] LookAheadSequence { get; internal set; }

    /// <summary>
    /// Gets an array of positioning lookups, in design order.
    /// </summary>
    public RuleLookupRecord[] Records { get; internal set; }

    internal ChainRuleTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
        base(reader, log, parent, offset)
    {
        ushort backtrackGlyphCount = reader.ReadUInt16();
        BacktrackSequence = reader.ReadArray<ushort>(backtrackGlyphCount);

        ushort inputGlyphCount = reader.ReadUInt16();

        if (inputGlyphCount == 0)
            InputSequence = new ushort[0];
        else
            InputSequence = reader.ReadArray<ushort>(inputGlyphCount - 1);

        ushort lookAheadGlyphCount = reader.ReadUInt16();
        LookAheadSequence = reader.ReadArray<ushort>(lookAheadGlyphCount);

        ushort posCount = reader.ReadUInt16();
        Records = new RuleLookupRecord[posCount];
        for (int i = 0; i < posCount; i++)
        {
            Records[i] = new RuleLookupRecord(
                seqIndex: reader.ReadUInt16(),
                lookupIndex: reader.ReadUInt16()
            );
        }
    }
}
