using Molten.IO;

namespace Molten.Font
{
    public class LigatureArrayTable : FontSubTable
    {
        public LigatureAttachTable[] Tables { get; private set; }

        internal LigatureArrayTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, long markClassCount) :
            base(reader, log, parent, offset)
        {
            ushort ligatureCount = reader.ReadUInt16();
            ushort[] offsets = reader.ReadArray<ushort>(ligatureCount);
            Tables = new LigatureAttachTable[ligatureCount];

            for (int i = 0; i < ligatureCount; i++)
                Tables[i] = new LigatureAttachTable(reader, log, this, offsets[i], markClassCount);
        }
    }

    public class LigatureAttachTable : FontSubTable
    {
        public ComponentRecord[] Records { get; private set; }

        internal LigatureAttachTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, long markClassCount) :
            base(reader, log, parent, offset)
        {
            ushort componentCount = reader.ReadUInt16();
            ushort[,] offsets = new ushort[componentCount, markClassCount];
            Records = new ComponentRecord[componentCount];

            // Collect all the record anchor table offsets.
            for (int i = 0; i < componentCount; i++)
            {
                for (int j = 0; j < markClassCount; j++)
                    offsets[i, j] = reader.ReadUInt16();
            }

            // Now build the records and read their anchor tables.
            for (int i = 0; i < componentCount; i++)
            {
                AnchorTable[] tables = new AnchorTable[markClassCount];
                for (int j = 0; j < markClassCount; j++)
                    tables[j] = new AnchorTable(reader, log, this, offsets[i, j]);

                Records[i] = new ComponentRecord() { AnchorTables = tables };
            }
        }
    }

    public class ComponentRecord
    {
        public AnchorTable[] AnchorTables { get; internal set; }
    }
}
