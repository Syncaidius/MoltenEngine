using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class MarkArrayTable
    {
        public MarkRecord[] Records { get; private set; }

        internal MarkArrayTable(BinaryEndianAgnosticReader reader, Logger log, long startPos)
        {
            reader.Position = startPos;
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
                Records[i].Table = new AnchorTable(reader, log, startPos + anchorOffsets[i]);
        }
    }

    public class MarkRecord
    {
        public ushort MarkClass { get; internal set; }

        public AnchorTable Table { get; internal set; }
    }
}
