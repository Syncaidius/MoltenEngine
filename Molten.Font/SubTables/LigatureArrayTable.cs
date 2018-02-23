using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class LigatureArrayTable
    {
        public LigatureAttachTable[] Tables { get; private set; }

        internal LigatureArrayTable(BinaryEndianAgnosticReader reader, Logger log, long startPos, long markClassCount)
        {
            reader.Position = startPos;
            ushort ligatureCount = reader.ReadUInt16();
            ushort[] offsets = reader.ReadArrayUInt16(ligatureCount);
            Tables = new LigatureAttachTable[ligatureCount];

            for (int i = 0; i < ligatureCount; i++)
                Tables[i] = new LigatureAttachTable(reader, log, startPos + offsets[i], markClassCount);
        }
    }

    public class LigatureAttachTable
    {
        public ComponentRecord[] Records { get; private set; }

        internal LigatureAttachTable(BinaryEndianAgnosticReader reader, Logger log, long startPos, long markClassCount)
        {
            reader.Position = startPos;
            ushort componentCount = reader.ReadUInt16();
            ushort[,] offsets = new ushort[componentCount, markClassCount];
            Records = new ComponentRecord[componentCount];

            // Collect all the record anchor table offsets.
            for(int i = 0; i < componentCount; i++)
            {
                for(int j = 0; j < markClassCount; j++)
                    offsets[i, j] = reader.ReadUInt16();
            }

            // Now build the records and read their anchor tables.
            for(int i = 0; i < componentCount; i++)
            {
                AnchorTable[] tables = new AnchorTable[markClassCount];
                for (int j = 0; j < markClassCount; j++)
                    tables[j] = new AnchorTable(reader, log, startPos + offsets[i, j]);

                Records[i] = new ComponentRecord() { AnchorTables = tables };
            }
        }
    }

    public class ComponentRecord
    {
        public AnchorTable[] AnchorTables { get; internal set; }
    }
}
