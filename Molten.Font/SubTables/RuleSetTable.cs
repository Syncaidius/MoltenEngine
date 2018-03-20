using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>
    /// See for PosRuleSetTable: https://docs.microsoft.com/en-us/typography/opentype/spec/gpos#context-positioning-subtable-format-1-simple-glyph-contexts <para/>
    /// See for SubRuleSetTable: https://docs.microsoft.com/en-us/typography/opentype/spec/gsub#51-context-substitution-format-1-simple-glyph-contexts
    /// </summary>
    public class RuleSetTable : FontSubTable
    {
        /// <summary>
        /// Gets an array of PosRule tables, ordered by preference.
        /// </summary>
        public RuleTable[] Tables { get; internal set; }

        internal override void Read(EnhancedBinaryReader reader, FontReaderContext context, FontTable parent)
        {
            ushort posRuleCount = reader.ReadUInt16();
            ushort[] posRuleOffsets = reader.ReadArray<ushort>(posRuleCount);
            Tables = new RuleTable[posRuleCount];
            for (int i = 0; i < posRuleCount; i++)
                Tables[i] = context.ReadSubTable<RuleTable>(posRuleOffsets[i]);
        }
    }

    /// <summary>
    /// A PosRule table also contains a count of the positioning operations to be performed on the input glyph sequence (posCount) and an array of PosLookupRecords (posLookupRecords). 
    /// Each record specifies a position in the input glyph sequence and a LookupList index to the positioning lookup to be applied there. 
    /// The array should list records in design order, or the order the lookups should be applied to the entire glyph sequence.<para/>
    /// See for PosRuleTable: https://docs.microsoft.com/en-us/typography/opentype/spec/gpos#context-positioning-subtable-format-1-simple-glyph-contexts <para/>
    /// See for SubRuleTable: https://docs.microsoft.com/en-us/typography/opentype/spec/gsub#51-context-substitution-format-1-simple-glyph-contexts
    /// </summary>
    public class RuleTable : FontSubTable
    {
        /// <summary>
        /// Gets an array of input glyph IDs — starting with the second glyph.
        /// </summary>
        public ushort[] InputSequence { get; internal set; }

        /// <summary>
        /// Gets an array of positioning lookups, in design order.
        /// </summary>
        public RuleLookupRecord[] Records { get; internal set; }

        internal override void Read(EnhancedBinaryReader reader, FontReaderContext context, FontTable parent)
        {
            ushort glyphCount = reader.ReadUInt16();
            ushort substitutionCount = reader.ReadUInt16();
            InputSequence = reader.ReadArray<ushort>(glyphCount - 1);
            Records = new RuleLookupRecord[substitutionCount];
            for (int i = 0; i < substitutionCount; i++)
            {
                Records[i] = new RuleLookupRecord(
                    seqIndex: reader.ReadUInt16(),
                    lookupIndex: reader.ReadUInt16()
                );
            }
        }
    }

    /// <summary>
    /// See for SubLookupRecord: https://docs.microsoft.com/en-us/typography/opentype/spec/gsub#substitution-lookup-record <para/>
    /// See for PosLookupRecord: https://docs.microsoft.com/en-us/typography/opentype/spec/gpos#position-lookup-record
    /// </summary>
    public struct RuleLookupRecord
    {
        /// <summary>
        /// Gets an index to the current glyph sequence — first glyph = 0.
        /// </summary>
        public readonly ushort SequenceIndex;

        /// <summary>
        /// Gets a lookup to apply to that position — zero-based index.
        /// </summary>
        public readonly ushort LookupListIndex;

        internal RuleLookupRecord(ushort seqIndex, ushort lookupIndex)
        {
            SequenceIndex = seqIndex;
            LookupListIndex = lookupIndex;
        }
    }
}
