using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Font
{
    /// <summary>Glyph definition table. See: https://www.microsoft.com/typography/otspec/gdef.htm
    public class GDEF : FontTable
    {
        public ushort MajorVersion { get; internal set; }

        public ushort MinorVersion { get; internal set; }

        public ClassDefinitionTable<GlyphClassDefinition> GlyphClassDefs { get; internal set; }

        public AttachListTable AttachList { get; internal set; }

        public ushort LigCaretListOffset { get; internal set; }

        public ushort MarkAttachClassDefOffset { get; internal set; }

        /// <summary>Set in table version 1.2 or higher, otherwise 0.</summary>
        public ushort MarkGlyphSetsDefOffset { get; internal set; }

        /// <summary>Set in table version 1.3 or higher, otherwise 0.</summary>
        public ushort ItemVarStoreOffset { get; internal set; }

        public class Parser : FontTableParser
        {
            static readonly GlyphClassDefinition[] _classTranslation = ReflectionHelper.EnumToArray<GlyphClassDefinition>();

            public override string TableTag => "GDEF";

            public override FontTable Parse(BinaryEndianAgnosticReader reader, TableHeader header, Logger log)
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

                // Read version-specific table information.
                if (table.MajorVersion >= 1)
                {
                    if(table.MinorVersion >= 2)
                        table.MarkGlyphSetsDefOffset = reader.ReadUInt16();

                    if (table.MinorVersion >= 3)
                        table.ItemVarStoreOffset = reader.ReadUInt16();
                }

                // Glyph class definition table
                if (glyphClassDefOffset > 0)
                {
                    log.WriteDebugLine($"[GDEF] Reading Glyph Class Definition Table -- Local pos: {glyphClassDefOffset}/{header.Length}");
                    reader.Position = header.Offset + glyphClassDefOffset;
                    table.GlyphClassDefs = new ClassDefinitionTable<GlyphClassDefinition>();
                    table.GlyphClassDefs.ReadTable(reader, log, header, _classTranslation);
                }

                // Attachment point list table
                /*The table consists of an offset to a Coverage table (Coverage) listing all glyphs that define attachment points in the GPOS table, 
                 * a count of the glyphs with attachment points (GlyphCount), and an array of offsets to AttachPoint tables (AttachPoint). 
                 * The array lists the AttachPoint tables, one for each glyph in the Coverage table, in the same order as the Coverage Index.*/
                if (attachListOffset > 0)
                {
                    log.WriteDebugLine($"[GDEF] Reading Attachment Point List Table -- Local pos: {attachListOffset}/{header.Length}");
                    reader.Position = header.Offset + attachListOffset;
                    table.AttachList = new AttachListTable();
                    table.AttachList.ReadTable(reader, log, header);
                }

                return table;
            }
        }
    }
}
