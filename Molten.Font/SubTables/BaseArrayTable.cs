using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class BaseArrayTable : FontSubTable
    {
        public BaseRecord[] Records { get; private set; }

        internal BaseArrayTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, ushort markClassCount)
            : base(reader, log, parent, offset)
        {
            ushort baseCount = reader.ReadUInt16();
            Records = new BaseRecord[baseCount];
            ushort[][] anchorOffsets = new ushort[baseCount][];

            for (int i = 0; i < baseCount; i++)
                anchorOffsets[i] = reader.ReadArray<ushort>(markClassCount);

            // Read anchor tables.
            for (int i = 0; i < baseCount; i++)
            {
                AnchorTable[] tables = new AnchorTable[markClassCount];
                for (int j = 0; j < markClassCount; j++)
                    tables[j] = new AnchorTable(reader, log, this, anchorOffsets[i][j]);

                Records[i] = new BaseRecord() { Tables = tables };
            }
        }
    }

    public class BaseRecord
    {
        public AnchorTable[] Tables { get; internal set; }
    }
}
