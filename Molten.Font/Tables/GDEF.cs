using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Glyph definition table. <para/>
    /// See: https://www.microsoft.com/typography/otspec/gdef.htm </summary>
    [FontTableTag("GDEF")]
    public class GDEF : MainFontTable
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
        public ClassDefinitionTable MarkAttachClassDefs { get; internal set; }

        /// <summary> Gets the GDEF mark glyph sets sub-table. <para />
        /// Set in table version 1.2 or higher, otherwise null. <para />
        /// See: https://www.microsoft.com/typography/otspec/gdef.htm</summary>
        public MarkGlyphSetsTable MarkGlyphSets { get; internal set; }

        /// <summary>Gets the item variable store, if available. <para/>
        /// The Item Variation Store contains adjustment-delta values arranged in one or more sets of deltas that are referenced using delta-set indices. <para/>
        /// Set in table version 1.3 or higher, otherwise null.</summary>
        public ItemVariationStore ItemVarStore { get; internal set; }

        static readonly GlyphClass[] _classTranslation = ReflectionHelper.EnumToArray<GlyphClass>();
        static readonly GlyphMarkClass[] _markTranslation = ReflectionHelper.EnumToArray<GlyphMarkClass>();

        internal override void Read(EnhancedBinaryReader reader, FontReaderContext context, TableHeader header, FontTableList dependencies)
        {
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();

            ushort glyphClassDefOffset = reader.ReadUInt16();
            ushort attachListOffset = reader.ReadUInt16();
            ushort ligCaretListOffset = reader.ReadUInt16();
            ushort markAttachClassDefOffset = reader.ReadUInt16();
            ushort markGlyphSetsDefOffset = 0;
            ushort itemVarStoreOffset = 0;

            // Read version-specific table information.
            if (MajorVersion >= 1)
            {
                if (MinorVersion >= 2)
                    markGlyphSetsDefOffset = reader.ReadUInt16();

                if (MinorVersion >= 3)
                    itemVarStoreOffset = reader.ReadUInt16();
            }

            // Glyph class definition table
            if(glyphClassDefOffset > FontUtil.NULL)
                GlyphClassDefs = new ClassDefinitionTable<GlyphClass>(reader, log, this, glyphClassDefOffset, _classTranslation);

            // Attachment point list table
            /*The table consists of an offset to a Coverage table (Coverage) listing all glyphs that define attachment points in the GPOS table, 
             * a count of the glyphs with attachment points (GlyphCount), and an array of offsets to AttachPoint tables (AttachPoint). 
             * The array lists the AttachPoint tables, one for each glyph in the Coverage table, in the same order as the Coverage Index.*/
            if (attachListOffset > FontUtil.NULL)
                AttachList = context.ReadSubTable<AttachListTable>(attachListOffset);

            // Ligature caret list sub-
            if (ligCaretListOffset > FontUtil.NULL)
                LigatureCaretList = context.ReadSubTable<LigatureCaretListTable>(ligCaretListOffset);

            // Mark attachment class definition sub-
            if (markAttachClassDefOffset > FontUtil.NULL)
                MarkAttachClassDefs = context.ReadSubTable<ClassDefinitionTable>(markAttachClassDefOffset);

            // Mark glyph sets sub-
            if (markGlyphSetsDefOffset > FontUtil.NULL)
                MarkGlyphSets = context.ReadSubTable<MarkGlyphSetsTable>(markGlyphSetsDefOffset);

            // Item variation store sub-
            if (itemVarStoreOffset > FontUtil.NULL)
                ItemVarStore = context.ReadSubTable<ItemVariationStore>(itemVarStoreOffset);
        }
    }
}
