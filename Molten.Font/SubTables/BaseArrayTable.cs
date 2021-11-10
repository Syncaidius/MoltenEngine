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
            ushort[,] anchorOffsets = new ushort[baseCount, markClassCount];

            for (int i = 0; i < baseCount; i++)
            {
                for (int j = 0; j < markClassCount; j++)
                    anchorOffsets[i, j] = reader.ReadUInt16();
            }

            // Read anchor tables.
            for (int i = 0; i < baseCount; i++)
            {
                AnchorTable[] tables = new AnchorTable[markClassCount];
                for (int j = 0; j < markClassCount; j++)
                    tables[j] = new AnchorTable(reader, log, this, anchorOffsets[i, j]);

                Records[i] = new BaseRecord() { Tables = tables };
            }
        }
    }

    public class BaseRecord
    {
        public AnchorTable[] Tables { get; internal set; }
    }
}
