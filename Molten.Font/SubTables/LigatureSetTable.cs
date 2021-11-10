namespace Molten.Font
{
    public class LigatureSetTable : FontSubTable
    {
        /// <summary>
        /// Gets an array of <see cref="LigatureTable"/>, ordered by preference.
        /// </summary>
        public LigatureTable[] Tables { get; internal set; }

        internal LigatureSetTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
            base(reader, log, parent, offset)
        {
            ushort ligatureCount = reader.ReadUInt16();
            ushort[] ligatureOffsets = reader.ReadArray<ushort>(ligatureCount);
            Tables = new LigatureTable[ligatureCount];
            for (int i = 0; i < ligatureCount; i++)
                Tables[i] = new LigatureTable(reader, log, this, ligatureOffsets[i]);
        }
    }

    public class LigatureTable : FontSubTable
    {
        /// <summary>
        /// Gets the glyph ID of the ligature to substitute.
        /// </summary>
        public ushort LigatureGlyph { get; private set; }

        /// <summary>
        /// Gets an array of component glyph IDs — starting with the second component, ordered in writing direction.
        /// </summary>
        public ushort[] ComponentGlyphIDs { get; private set; }

        internal LigatureTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
            base(reader, log, parent, offset)
        {
            LigatureGlyph = reader.ReadUInt16();
            ushort componentCount = reader.ReadUInt16();
            ComponentGlyphIDs = reader.ReadArray<ushort>(componentCount - 1);
        }
    }
}
