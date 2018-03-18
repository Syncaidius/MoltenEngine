using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class MarkArrayTable : FontSubTable
    {
        public MarkRecord[] Records { get; private set; }

        internal MarkArrayTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
            base(reader, log, parent, offset)
        {
            ushort markCount = reader.ReadUInt16();
            Records = new MarkRecord[markCount];
            ushort[] anchorOffsets = new ushort[markCount];

            // Read mark record info (class + anchor offset per record).
            for(int i = 0; i < markCount; i++)
            {
                Records[i] = new MarkRecord()
                {
                    MarkClass = reader.ReadUInt16(),
                };
                anchorOffsets[i] = reader.ReadUInt16();
            }

            // Read anchor tables.
            for(int i = 0; i < markCount; i++)
                Records[i].Table = new AnchorTable(reader, log, this, anchorOffsets[i]);
        }
    }

    public class MarkRecord
    {
        public ushort MarkClass { get; internal set; }

        public AnchorTable Table { get; internal set; }
    }

    public class Mark2ArrayTable : FontSubTable
    {
        public Mark2Record[] Records { get; private set; }

        internal Mark2ArrayTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, int markClassCount) :
            base(reader, log, parent, offset)
        {
            ushort mark2Count = reader.ReadUInt16();
            Records = new Mark2Record[mark2Count];
            ushort[][] anchorOffsets = new ushort[mark2Count][];

            // Read read offsets for each mark record. 
            // Since each record only contains an array of offsets, we can read them the same way.
            for (int i = 0; i < mark2Count; i++)
                anchorOffsets[i] = reader.ReadArray<ushort>(markClassCount);

            // Read anchor tables.
            for (int i = 0; i < mark2Count; i++)
            {
                Records[i].Tables = new AnchorTable[markClassCount];
                for(int j = 0; j < markClassCount; j++)
                    Records[i].Tables[j] = new AnchorTable(reader, log, this, anchorOffsets[i][j]);
            }
        }
    }

    public class Mark2Record
    {
        /// <summary>
        /// Gets an Array of <see cref="AnchorTable"/>, in class order.
        /// </summary>
        public AnchorTable[] Tables { get; internal set; }
    }
}
