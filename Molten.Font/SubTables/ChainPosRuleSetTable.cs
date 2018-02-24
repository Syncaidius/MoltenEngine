using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class ChainPosRuleSetTable : FontSubTable
    {
        /// <summary>
        /// Gets an array of PosRule tables, ordered by preference.
        /// </summary>
        public ChainPosRuleTable[] Tables { get; internal set; }

        internal ChainPosRuleSetTable(BinaryEndianAgnosticReader reader, Logger log, IFontTable parent, long offset) :
            base(reader, log, parent, offset)
        {
            ushort posRuleCount = reader.ReadUInt16();
            ushort[] posRuleOffsets = reader.ReadArrayUInt16(posRuleCount);
            Tables = new ChainPosRuleTable[posRuleCount];
            for (int i = 0; i < posRuleCount; i++)
                Tables[i] = new ChainPosRuleTable(reader, log, this, posRuleOffsets[i]);
        }
    }

    /// <summary>
    /// A PosRule table also contains a count of the positioning operations to be performed on the input glyph sequence (posCount) and an array of PosLookupRecords (posLookupRecords). 
    /// Each record specifies a position in the input glyph sequence and a LookupList index to the positioning lookup to be applied there. 
    /// The array should list records in design order, or the order the lookups should be applied to the entire glyph sequence.
    /// </summary>
    public class ChainPosRuleTable : FontSubTable
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
        public PosLookupRecord[] Records { get; internal set; }

        internal ChainPosRuleTable(BinaryEndianAgnosticReader reader, Logger log, IFontTable parent, long offset) :
            base(reader, log, parent, offset)
        {
            ushort backtrackGlyphCount = reader.ReadUInt16();
            BacktrackSequence = reader.ReadArrayUInt16(backtrackGlyphCount);

            ushort inputGlyphCount = reader.ReadUInt16();
            InputSequence = reader.ReadArrayUInt16(inputGlyphCount - 1);

            ushort lookAheadGlyphCount = reader.ReadUInt16();
            LookAheadSequence = reader.ReadArrayUInt16(lookAheadGlyphCount);

            ushort posCount = reader.ReadUInt16();
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
}
