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

        public ushort GlyphClassDefOffset { get; internal set; }

        public ushort AttachListOffset { get; internal set; }

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
                    GlyphClassDefOffset = reader.ReadUInt16(),
                    AttachListOffset = reader.ReadUInt16(),
                    LigCaretListOffset = reader.ReadUInt16(),
                    MarkAttachClassDefOffset = reader.ReadUInt16(),
                };

                // Read version-specific table information.
                if(table.MajorVersion >= 1)
                {
                    if(table.MinorVersion >= 2)
                        table.MarkGlyphSetsDefOffset = reader.ReadUInt16();

                    if (table.MinorVersion >= 3)
                        table.ItemVarStoreOffset = reader.ReadUInt16();
                }

                reader.Position = header.Offset + table.GlyphClassDefOffset;
                ClassDefinitionTable<GlyphClassDefinition> glyphClassDef = new ClassDefinitionTable<GlyphClassDefinition>();
                glyphClassDef.ReadTable(reader, log, header, _classTranslation);

                return table;
            }
        }
    }
}
