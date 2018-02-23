using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class PosClassSetTable
    {
        /// <summary>
        /// Gets an array of PosClassRule tables, ordered by preference.
        /// </summary>
        public PosClassRuleTable[] Tables { get; internal set; }

        internal PosClassSetTable(BinaryEndianAgnosticReader reader, Logger log, long startPos)
        {
            reader.Position = startPos;
            ushort posClassRuleCount = reader.ReadUInt16();
            ushort[] posClassRuleOffsets = reader.ReadArrayUInt16(posClassRuleCount);
            Tables = new PosClassRuleTable[posClassRuleCount];
            for (int i = 0; i < posClassRuleCount; i++)
                Tables[i] = new PosClassRuleTable(reader, log, startPos + posClassRuleOffsets[i]);
        }
    }

    /// <summary>
    /// A PosClassRule also contains a count of the positioning operations to be performed on the context (PosCount) and an array of PosLookupRecords (PosLookupRecord) that supply the positioning data. 
    /// For each position in the context that requires a positioning operation, a PosLookupRecord specifies a LookupList index and a position in the input glyph class sequence where the lookup is applied. 
    /// The PosLookupRecord array lists PosLookupRecords in design order, or the order in which lookups are applied to the entire glyph sequence.
    /// </summary>
    public class PosClassRuleTable
    {
        /// <summary>
        /// Gets an array of classes to be matched to the input glyph sequence, beginning with the second glyph position.
        /// </summary>
        public ushort[] Classes { get; internal set; }

        /// <summary>
        /// Gets an array of positioning lookups, in design order.
        /// </summary>
        public PosLookupRecord[] Records { get; internal set; }

        internal PosClassRuleTable(BinaryEndianAgnosticReader reader, Logger log, long startPos)
        {
            reader.Position = startPos;
            ushort glyphCount = reader.ReadUInt16();
            ushort posCount = reader.ReadUInt16();
            Classes = reader.ReadArrayUInt16(glyphCount - 1);
            Records = new PosLookupRecord[posCount];
            for (int i = 0; i < posCount; i++)
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
