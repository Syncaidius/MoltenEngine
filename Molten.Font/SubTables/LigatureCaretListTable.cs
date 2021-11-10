namespace Molten.Font
{
    public class LigatureCaretListTable : FontSubTable
    {
        /// <summary>Gets an array containing AttachPoint tables ordered by coverage index, which hold contour point indices.</summary>
        public LigatureGlyphTable[] GlyphTables { get; private set; }

        internal LigatureCaretListTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
            base(reader, log, parent, offset)
        {
            ushort coverageOffset = reader.ReadUInt16();
            ushort ligGlyphCount = reader.ReadUInt16();
            ushort[] ligGlyphOffsets = reader.ReadArray<ushort>(ligGlyphCount);

            // Read the coverage table.
            CoverageTable coverage = new CoverageTable(reader, log, this, coverageOffset);
            GlyphTables = new LigatureGlyphTable[ligGlyphCount];

            // Populate attach points in each AttachPointTable.
            for (int i = 0; i < ligGlyphCount; i++)
                GlyphTables[i] = new LigatureGlyphTable(reader, log, this, ligGlyphOffsets[i], coverage);
        }
    }
}
