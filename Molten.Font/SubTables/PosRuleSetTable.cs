using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class PosRuleSetTable : FontSubTable
    {
        /// <summary>
        /// Gets an array of PosRule tables, ordered by preference.
        /// </summary>
        public PosRuleTable[] Tables { get; internal set; }

        internal PosRuleSetTable(BinaryEndianAgnosticReader reader, Logger log, IFontTable parent, long offset) :
            base(reader, log, parent, offset)
        {
            ushort posRuleCount = reader.ReadUInt16();
            ushort[] posRuleOffsets = reader.ReadArrayUInt16(posRuleCount);
            Tables = new PosRuleTable[posRuleCount];
            for (int i = 0; i < posRuleCount; i++)
                Tables[i] = new PosRuleTable(reader, log, this, posRuleOffsets[i]);
        }
    }

    /// <summary>
    /// A PosRule table also contains a count of the positioning operations to be performed on the input glyph sequence (posCount) and an array of PosLookupRecords (posLookupRecords). 
    /// Each record specifies a position in the input glyph sequence and a LookupList index to the positioning lookup to be applied there. 
    /// The array should list records in design order, or the order the lookups should be applied to the entire glyph sequence.
    /// </summary>
    public class PosRuleTable : FontSubTable
    {
        /// <summary>
        /// Gets an array of input glyph IDs — starting with the second glyph.
        /// </summary>
        public ushort[] InputSequence { get; internal set; }

        /// <summary>
        /// Gets an array of positioning lookups, in design order.
        /// </summary>
        public PosLookupRecord[] Records { get; internal set; }

        internal PosRuleTable(BinaryEndianAgnosticReader reader, Logger log, IFontTable parent, long offset):
            base(reader, log, parent, offset)
        {
            ushort glyphCount = reader.ReadUInt16();
            ushort posCount = reader.ReadUInt16();
            InputSequence = reader.ReadArrayUInt16(glyphCount - 1);
            Records = new PosLookupRecord[posCount];
            for(int i = 0; i < posCount; i++)
            {
                Records[i] = new PosLookupRecord()
                {
                    SequenceIndex = reader.ReadUInt16(),
                    LookupListIndex = reader.ReadUInt16(),
                };
            }
        }
    }

    public class PosLookupRecord
    {
        /// <summary>
        /// Gets the index (zero-based) to input glyph sequence
        /// </summary>
        public ushort SequenceIndex { get; internal set; }

        /// <summary>
        /// Gets the index (zero-based) into the LookupList for the Lookup table to apply to that position in the glyph sequence.
        /// </summary>
        public ushort LookupListIndex { get; internal set; }
    }
}
