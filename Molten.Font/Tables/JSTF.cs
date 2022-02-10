using Molten.IO;

namespace Molten.Font
{
    /// <summary>JSTF — Justification Table <para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/jstf </summary>
    [FontTableTag("JSTF")]
    public class JSTF : FontTable
    {
        public ushort MajorVersion { get; private set; }

        public ushort MinorVersion { get; private set; }

        public JustificationScriptTable[] Scripts { get; private set; }

        internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
        {
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
            ushort scriptCount = reader.ReadUInt16();

            // Populate script records
            JstfRecord[] records = new JstfRecord[scriptCount];
            for (int i = 0; i < scriptCount; i++)
            {
                records[i] = new JstfRecord()
                {
                    ScriptTag = FontUtil.ReadTag(reader),
                    Offset = reader.ReadUInt16(),
                };
            }

            Scripts = new JustificationScriptTable[scriptCount];
            for (int i = 0; i < scriptCount; i++)
                Scripts[i] = new JustificationScriptTable(reader, log, this, records[i]);
        }
    }

    internal struct JstfRecord
    {
        public string ScriptTag;

        public ushort Offset;
    }

    /// <summary>
    /// A Justification Script (JstfScript) table describes the justification information for a single script. <para/>
    /// It consists of an offset to a table that defines extender glyphs (extenderGlyphOffset), an offset to a default justification table for the script (defJstfLangSysOffset), and a count of the language systems that define justification data (jstfLangSysCount).
    /// </summary>
    public class JustificationScriptTable : FontSubTable
    {
        /// <summary>
        /// Gets the <see cref="ExtenderGlyphTable"/> (may be null).
        /// </summary>
        public ExtenderGlyphTable ExtenderGlyphs { get; private set; }

        /// <summary>
        /// Gets the default <see cref="JustificationLanguageSystemTable"/> (may be null).
        /// </summary>
        public JustificationLanguageSystemTable DefaultLanguageSystem { get; private set; }

        /// <summary>
        /// Gets an array of <see cref="JustificationLanguageSystemTable"/>, in alphabetical order by <see cref="JustificationLanguageSystemTable.Tag"/>
        /// </summary>
        public JustificationLanguageSystemTable[] LanguageSystems { get; private set; }

        internal JustificationScriptTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, JstfRecord record) :
            base(reader, log, parent, record.Offset)
        {
            ushort extenderGlyphOffset = reader.ReadUInt16();
            ushort defaultJstfLangSysOffset = reader.ReadUInt16();
            ushort langSysCount = reader.ReadUInt16();

            JstfRecord[] records = new JstfRecord[langSysCount];
            for (int i = 0; i < langSysCount; i++)
            {
                records[i] = new JstfRecord()
                {
                    ScriptTag = FontUtil.ReadTag(reader),
                    Offset = reader.ReadUInt16(),
                };
            }

            if (!FontUtil.IsNull(extenderGlyphOffset))
                ExtenderGlyphs = new ExtenderGlyphTable(reader, log, this, extenderGlyphOffset);

            if (!FontUtil.IsNull(defaultJstfLangSysOffset))
            {
                DefaultLanguageSystem = new JustificationLanguageSystemTable(reader, log, this,
                    new JstfRecord() { Offset = defaultJstfLangSysOffset, ScriptTag = "default" });
            }

            LanguageSystems = new JustificationLanguageSystemTable[langSysCount];
            for (int i = 0; i < langSysCount; i++)
                LanguageSystems[i] = new JustificationLanguageSystemTable(reader, log, this, records[i]);
        }
    }

    /// <summary>
    /// Extender glyph table.<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/jstf#extender-glyph-table
    /// </summary>
    public class ExtenderGlyphTable : FontSubTable
    {
        /// <summary>
        /// Gets an array of extender glyph IDs — in increasing numerical order.
        /// </summary>
        public ushort[] Glyphs { get; private set; }

        internal ExtenderGlyphTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
            base(reader, log, parent, offset)
        {
            ushort glyphCount = reader.ReadUInt16();
            Glyphs = reader.ReadArray<ushort>(glyphCount);
        }
    }

    /// <summary>Justifcation language system table. <para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/jstf#justification-language-system-table
    /// </summary>
    public class JustificationLanguageSystemTable : FontSubTable
    {
        /// <summary>
        /// Gets an array of of <see cref="JustificationPriorityTable"/> instances, in priority order
        /// </summary>
        public JustificationPriorityTable[] PriorityTables { get; private set; }

        public string Tag { get; private set; }

        internal JustificationLanguageSystemTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, JstfRecord record) :
            base(reader, log, parent, record.Offset)
        {
            Tag = record.ScriptTag;
            ushort priorityCount = reader.ReadUInt16();
            ushort[] priorityOffsets = reader.ReadArray<ushort>(priorityCount);

            PriorityTables = new JustificationPriorityTable[priorityCount];
            for (int i = 0; i < priorityCount; i++)
                PriorityTables[i] = new JustificationPriorityTable(reader, log, this, priorityOffsets[i]);
        }

        public override string ToString()
        {
            return $"{base.ToString()} -- {Tag}";
        }
    }

    /// <summary>
    /// Justification priority table.<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/jstf#justification-priority-table
    /// </summary>
    public class JustificationPriorityTable : FontSubTable
    {
        public JustificationModListTable ShrinkageEnableGSUB { get; private set; }

        public JustificationModListTable ShrinkageDisableGSUB { get; private set; }

        public JustificationModListTable ShrinkageEnableGPOS { get; private set; }

        public JustificationModListTable ShrinkageDisableGPOS { get; private set; }

        public JustificationMaximumTable ShrinkageJustificationMax { get; private set; }

        public JustificationModListTable ExtensionEnableGSUB { get; private set; }

        public JustificationModListTable ExtensionDisableGSUB { get; private set; }

        public JustificationModListTable ExtensionEnableGPOS { get; private set; }

        public JustificationModListTable ExtensionDisableGPOS { get; private set; }

        public JustificationMaximumTable ExtensionJustificationMax { get; private set; }

        internal JustificationPriorityTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
            base(reader, log, parent, offset)
        {
            ushort shrinkageEnableGSUBOffset = reader.ReadUInt16();
            ushort shrinkageDisableGSUBOffset = reader.ReadUInt16();
            ushort shrinkageEnableGPOSOffset = reader.ReadUInt16();
            ushort shrinkageDisableGPOSOffset = reader.ReadUInt16();
            ushort shrinkagejstfMaxOffset = reader.ReadUInt16();
            ushort extensionEnableGSUBOffset = reader.ReadUInt16();
            ushort extensionDisableGSUBOffset = reader.ReadUInt16();
            ushort extensionEnableGPOSOffset = reader.ReadUInt16();
            ushort extensionDisableGPOSOffset = reader.ReadUInt16();
            ushort extensionJstfMaxOffset = reader.ReadUInt16();

            if (!FontUtil.IsNull(shrinkageEnableGSUBOffset))
                ShrinkageEnableGSUB = new JustificationModListTable(reader, log, this, shrinkageEnableGSUBOffset);
            if (!FontUtil.IsNull(shrinkageDisableGSUBOffset))
                ShrinkageDisableGSUB = new JustificationModListTable(reader, log, this, shrinkageDisableGSUBOffset);

            if (!FontUtil.IsNull(shrinkageEnableGPOSOffset))
                ShrinkageEnableGPOS = new JustificationModListTable(reader, log, this, shrinkageEnableGPOSOffset);
            if (!FontUtil.IsNull(shrinkageDisableGPOSOffset))
                ShrinkageDisableGPOS = new JustificationModListTable(reader, log, this, shrinkageDisableGPOSOffset);

            if (!FontUtil.IsNull(shrinkagejstfMaxOffset))
                ShrinkageJustificationMax = new JustificationMaximumTable(reader, log, this, shrinkagejstfMaxOffset);

            if (!FontUtil.IsNull(extensionEnableGSUBOffset))
                ExtensionEnableGSUB = new JustificationModListTable(reader, log, this, extensionEnableGSUBOffset);
            if (!FontUtil.IsNull(extensionDisableGSUBOffset))
                ExtensionDisableGSUB = new JustificationModListTable(reader, log, this, extensionDisableGSUBOffset);

            if (!FontUtil.IsNull(extensionEnableGPOSOffset))
                ExtensionEnableGPOS = new JustificationModListTable(reader, log, this, extensionEnableGPOSOffset);
            if (!FontUtil.IsNull(extensionDisableGPOSOffset))
                ExtensionDisableGPOS = new JustificationModListTable(reader, log, this, extensionDisableGPOSOffset);

            if (!FontUtil.IsNull(shrinkagejstfMaxOffset))
                ExtensionJustificationMax = new JustificationMaximumTable(reader, log, this, shrinkagejstfMaxOffset);

        }
    }

    public class JustificationModListTable : FontSubTable
    {
        /// <summary>
        /// Gets an array of Lookup indices into the GSUB/GPOS LookupList, in increasing numerical order
        /// </summary>
        public ushort[] LookupIndices { get; private set; }

        internal JustificationModListTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
            base(reader, log, parent, offset)
        {
            ushort lookupCount = reader.ReadUInt16();
            LookupIndices = reader.ReadArray<ushort>(lookupCount);
        }
    }

    /// <summary>
    /// Justification maximum table. <para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/jstf#justification-maximum-table
    /// </summary>
    public class JustificationMaximumTable : FontSubTable
    {
        public JustificationLookupTable[] Lookups { get; private set; }

        internal JustificationMaximumTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
            base(reader, log, parent, offset)
        {
            ushort lookupCount = reader.ReadUInt16();
            ushort[] lookupOffsets = reader.ReadArray<ushort>(lookupCount);
        }
    }

    /// <summary>
    /// GPOS - Lookup Type 1: Single Adjustment Positioning Subtable. <para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/gpos#lookup-type-1-single-adjustment-positioning-subtable
    /// </summary>
    public class JustificationLookupTable : FontSubTable
    {
        /// <summary>Gets an array of positioning value records. Each record corresponds to the matching glyph in <see cref="Coverage"/>.</summary>
        public GPOS.ValueRecord[] Records { get; private set; }

        /// <summary>
        /// Gets the coverage table associated with the current <see cref="SingleAdjustmentPosTable"/>. This contains the IDs of glyphs to be adjusted.
        /// </summary>
        public CoverageTable Coverage { get; private set; }

        internal JustificationLookupTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
            base(reader, log, parent, offset)
        {
            ushort format = reader.ReadUInt16();
            ushort coverageOffset = reader.ReadUInt16();
            GPOS.ValueFormat valueFormat = (GPOS.ValueFormat)reader.ReadUInt16();

            switch (format)
            {
                case 1:
                    Records = new GPOS.ValueRecord[1];
                    Records[0] = new GPOS.ValueRecord(reader, valueFormat);
                    break;

                case 2:
                    ushort valueCount = reader.ReadUInt16();
                    Records = new GPOS.ValueRecord[valueCount];
                    for (int i = 0; i < valueCount; i++)
                        Records[i] = new GPOS.ValueRecord(reader, valueFormat);
                    break;

                default:
                    log.WriteDebugLine($"[GPOS] unsupported JustificationLookupTable format {format}");
                    break;
            }

            Coverage = new CoverageTable(reader, log, this, coverageOffset);
        }
    }
}
