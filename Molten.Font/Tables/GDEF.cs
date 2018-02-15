using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Glyph definition table. See: https://www.microsoft.com/typography/otspec/gdef.htm
    public class GDEF : FontTable
    {
        /// <summary>Gets the major version of the table.</summary>
        public ushort MajorVersion { get; internal set; }

        /// <summary>Gets the minor version of the table.</summary>
        public ushort MinorVersion { get; internal set; }

        /// <summary>Gets the glyph class definition sub-table if available, otherwise null.</summary>
        public ClassDefinitionTable<GlyphClass> GlyphClassDefs { get; internal set; }

        /// <summary>Gets the attachment point list sub-table if available, otherwise null.</summary>
        public AttachListTable AttachList { get; internal set; }

        /// <summary>Gets the ligature caret list sub-table if available, otherwise null.</summary>
        public LigatureCaretListTable LigatureCaretList { get; internal set; }

        /// <summary>Gets the glyph mark class definition sub-table if available, otherwise null.</summary>
        public ClassDefinitionTable<GlyphMarkClass> MarkAttachClassDefs { get; internal set; }

        /// <summary> Gets the GDEF mark glyph sets sub-table. <para />
        /// Set in table version 1.2 or higher, otherwise null. <para />
        /// See: https://www.microsoft.com/typography/otspec/gdef.htm</summary>
        public MarkGlyphSetsTable MarkGlyphSets { get; internal set; }

        /// <summary>Gets the item variable store, if available. <para/>
        /// The Item Variation Store contains adjustment-delta values arranged in one or more sets of deltas that are referenced using delta-set indices. <para/>
        /// Set in table version 1.3 or higher, otherwise null.</summary>
        public ItemVariationStore ItemVarStore { get; internal set; }

        internal class Parser : FontTableParser
        {
            static readonly GlyphClass[] _classTranslation = ReflectionHelper.EnumToArray<GlyphClass>();
            static readonly GlyphMarkClass[] _markTranslation = ReflectionHelper.EnumToArray<GlyphMarkClass>();

            public override string TableTag => "GDEF";

            internal override FontTable Parse(BinaryEndianAgnosticReader reader, TableHeader header, Logger log)
            {
                GDEF table = new GDEF()
                {
                    MajorVersion = reader.ReadUInt16(),
                    MinorVersion = reader.ReadUInt16(),
                };

                ushort glyphClassDefOffset = reader.ReadUInt16();
                ushort attachListOffset = reader.ReadUInt16();
                ushort ligCaretListOffset = reader.ReadUInt16();
                ushort markAttachClassDefOffset = reader.ReadUInt16();
                ushort markGlyphSetsDefOffset = 0;
                ushort itemVarStoreOffset = 0;

                // Read version-specific table information.
                if (table.MajorVersion >= 1)
                {
                    if (table.MinorVersion >= 2)
                        markGlyphSetsDefOffset = reader.ReadUInt16();

                    if (table.MinorVersion >= 3)
                        itemVarStoreOffset = reader.ReadUInt16();
                }

                // Glyph class definition table
                ReadSubTable(reader, log, "Glyph Class-Def", glyphClassDefOffset, header, (startPos) =>
                    table.GlyphClassDefs = new ClassDefinitionTable<GlyphClass>(reader, log, header, _classTranslation));

                // Attachment point list table
                /*The table consists of an offset to a Coverage table (Coverage) listing all glyphs that define attachment points in the GPOS table, 
                 * a count of the glyphs with attachment points (GlyphCount), and an array of offsets to AttachPoint tables (AttachPoint). 
                 * The array lists the AttachPoint tables, one for each glyph in the Coverage table, in the same order as the Coverage Index.*/
                ReadSubTable(reader, log, "Attachment Point List", attachListOffset, header, (startPos) =>
                table.AttachList = new AttachListTable(reader, log, header));

                // Ligature caret list sub-table.
                ReadSubTable(reader, log, "Ligature Caret List", ligCaretListOffset, header, (startPos) =>
                table.LigatureCaretList = new LigatureCaretListTable(reader, log, header));

                // Mark attachment class definition  sub-table.
                ReadSubTable(reader, log, "Mark Attach Class-Def", markAttachClassDefOffset, header, (startPos) =>
                table.MarkAttachClassDefs = new ClassDefinitionTable<GlyphMarkClass>(reader, log, header, _markTranslation));

                // Mark glyph sets  sub-table.
                ReadSubTable(reader, log, "Mark Glyph Set", markGlyphSetsDefOffset, header, (startPos) =>
                    table.MarkGlyphSets = new MarkGlyphSetsTable(reader, log, header));

                // Item variation store  sub-table.
                ReadSubTable(reader, log, "Item Variation Store", itemVarStoreOffset, header, (startPos) =>
                    table.ItemVarStore = new ItemVariationStore(reader, log, header));

                return table;
            }
        }
    }
}
