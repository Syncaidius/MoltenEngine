using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Index-to-location table.<para/>
    /// <para>The indexToLoc table stores the offsets to the locations of the glyphs in the font, relative to the beginning of the glyphData table. In order to compute the length of the last glyph element, there is an extra entry after the last valid index.</para>
    /// <para>By definition, index zero points to the "missing character," which is the character that appears if a character is not found in the font. The missing character is commonly represented by a blank box or a space. If the font does not contain an outline for the missing character, then the first and second offsets should have the same value. This also applies to any other characters without an outline, such as the space character. If a glyph has no outline, then loca[n] = loca [n+1]. In the particular case of the last glyph(s), loca[n] will be equal the length of the glyph data ('glyf') table. The offsets must be in ascending order with loca[n] less-or-equal-to loca[n+1].</para>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/gpos </summary>
    [FontTableTag("GPOS")]
    public partial class GPOS : FontGTable
    {
        Dictionary<GPOSLookupType, GPosLookupSubTable> _gposLookup = new Dictionary<GPOSLookupType, GPosLookupSubTable>();

        public GPosLookupSubTable GetLookupTable(GPOSLookupType type)
        {
            if (_gposLookup.TryGetValue(type, out GPosLookupSubTable subTable))
                return subTable;
            else
                return null;
        }

        protected override Type[] GetLookupTypeIndex()
        {
            return new Type[]
            {
                null, // Type 0
                typeof(SingleAdjustmentPosSubTable), // Type 1
                typeof(PairAdjustmentPosSubTable), // Type 2
                typeof(CursiveAttachmentPosSubTable), // Type 3
                typeof(MarkToBaseAttachmentPosSubTable), // Type 4,
                typeof(MarkToLigatureAttachmentPosSubTable), // Type 5,
                typeof(MarkToMarkAttachmentPosSubTable), // Type 6,
                typeof(ContextPosSubTable), // Type 7,
                typeof(ChainingContextualPosSubTable) // Type 8
            };
        }

        protected override ushort GetExtensionIndex()
        {
            return (ushort)GPOSLookupType.ExtensionPositioning;
        }
    }

    public enum GPOSLookupType : ushort
    {
        None = 0,

        SingleAdjustment = 1,

        PairAdjustment = 2,

        CursiveAttachment = 3,

        MarkToBaseAttachment = 4,

        MarkToLigatureAttachment = 5,

        MarkToMarkAttachment = 6,

        ContextPositioning = 7,

        ChainedContextPositioning = 8,

        ExtensionPositioning = 9,

        Reserved = 10,
    }

    /// <summary>
    /// A base class for all GPOS lookup tables.
    /// </summary>
    public abstract class GPosLookupSubTable : LookupTable
    {
        public GPOSLookupType Type { get; protected set; }

        public ushort Format { get; private set; }

        internal sealed override void Read(BinaryEndianAgnosticReader reader, Logger log, long startPos, ushort lookupType, LookupFlags flags, ushort markFilteringSet)
        {
            log.WriteDebugLine($"[GPOS] Parsing lookup sub-table '{this.GetType().Name}'");
            reader.Position = startPos;
            GPOSLookupType lt = (GPOSLookupType)lookupType;
            Type = lt;

            Format = reader.ReadUInt16();
            OnRead(reader, log, startPos, markFilteringSet, Format);
        }

        protected abstract void OnRead(BinaryEndianAgnosticReader reader, Logger log, long startPos, ushort markFilteringSet, ushort posFormat);
    }

    /// <summary>
    /// GPOS - Lookup Type 1: Single Adjustment Positioning Subtable. <para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/gpos#lookup-type-1-single-adjustment-positioning-subtable
    /// </summary>
    public class SingleAdjustmentPosSubTable : GPosLookupSubTable
    {
        /// <summary>Gets an array of positioning value records. Each record corresponds to the matching glyph in <see cref="Coverage"/>.</summary>
        public GPOS.ValueRecord[] Records { get; private set; }

        /// <summary>
        /// Gets the coverage table associated with the current <see cref="SingleAdjustmentPosSubTable"/>. This contains the IDs of glyphs to be adjusted.
        /// </summary>
        public CoverageTable Coverage { get; private set; }

        protected override void OnRead(BinaryEndianAgnosticReader reader, Logger log, long startPos, ushort markFilteringSet, ushort posFormat)
        {
            ushort coverageOffset = reader.ReadUInt16();
            GPOS.ValueFormat valueFormat = (GPOS.ValueFormat)reader.ReadUInt16();

            switch (posFormat)
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
            }

            Coverage = new CoverageTable(reader, log, startPos + coverageOffset);
        }
    }

    /// <summary>
    /// GPOS - Lookup Type 2: Pair Adjustment Positioning Subtable. <para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/gpos#lookup-type-2-pair-adjustment-positioning-subtable
    /// </summary>
    public class PairAdjustmentPosSubTable : GPosLookupSubTable
    {
        public GPOS.PairSet[] PairSets { get; internal set; }

        /// <summary>Gets an array containing <see cref="GPOS.Class1Record"/>.<para/>
        /// Each <see cref="GPOS.Class1Record"/> contains an array of <see cref="GPOS.Class2Record"/>, which also are ordered by class value. <para/>
        /// One <see cref="GPOS.Class2Record"/> must be declared for each class in the classDef2 table, including Class 0.</summary>
        public GPOS.Class1Record[] ClassRecords { get; internal set; }

        public CoverageTable Coverage { get; internal set; }

        public ClassDefinitionTable Class1Definitions { get; internal set; }

        public ClassDefinitionTable Class2Definitions { get; internal set; }

        protected override void OnRead(BinaryEndianAgnosticReader reader, Logger log, long startPos, ushort markFilteringSet, ushort posFormat)
        {
            ushort coverageOffset = reader.ReadUInt16();
            GPOS.ValueFormat valueFormat1 = (GPOS.ValueFormat)reader.ReadUInt16();
            GPOS.ValueFormat valueFormat2 = (GPOS.ValueFormat)reader.ReadUInt16();

            switch (posFormat)
            {
                case 1:
                    ushort pairSetCount = reader.ReadUInt16();
                    ushort[] pairSetOffsets = reader.ReadArrayUInt16(pairSetCount);
                    PairSets = new GPOS.PairSet[pairSetCount];
                    for (int i = 0; i < pairSetCount; i++)
                        PairSets[i] = new GPOS.PairSet(reader, startPos + pairSetOffsets[i], valueFormat1, valueFormat2);
                    break;

                case 2:
                    ushort classDef1Offset = reader.ReadUInt16();
                    ushort classDef2Offset = reader.ReadUInt16();
                    ushort classCount1 = reader.ReadUInt16();
                    ushort classCount2 = reader.ReadUInt16();

                    ClassRecords = new GPOS.Class1Record[classCount1];
                    for(int i = 0; i < classCount1; i++)
                        ClassRecords[i] = new GPOS.Class1Record(reader, classCount2, valueFormat1, valueFormat2);

                    Class1Definitions = new ClassDefinitionTable(reader, log, startPos + classDef1Offset);

                    // Check if Class2 table is the same as Class1. If true, share them.
                    if (classDef2Offset != classDef1Offset)
                        Class2Definitions = new ClassDefinitionTable(reader, log, startPos + classDef2Offset);
                    else
                        Class2Definitions = Class1Definitions;

                    break;
            }

            Coverage = new CoverageTable(reader, log, startPos + coverageOffset);
        }
    }

    /// <summary>
    /// Some cursive fonts are designed so that adjacent glyphs join when rendered with their default positioning. 
    /// However, if positioning adjustments are needed to join the glyphs, 
    /// a cursive attachment positioning (CursivePos) subtable can describe how to connect the glyphs by aligning two anchor points: 
    /// the designated exit point of a glyph, and the designated entry point of the following glyph.<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/gpos#lookup-type-3-cursive-attachment-positioning-subtable
    /// </summary>
    public class CursiveAttachmentPosSubTable : GPosLookupSubTable
    {
        public GPOS.EntryExitRecord[] Records { get; private set; }

        protected override void OnRead(BinaryEndianAgnosticReader reader, Logger log, long startPos, ushort markFilteringSet, ushort posFormat)
        {
            ushort coverageOffset = reader.ReadUInt16();
            ushort entryExitCount = reader.ReadUInt16();
            Records = new GPOS.EntryExitRecord[entryExitCount];
            for(int i = 0; i < entryExitCount; i++)
            {
                ushort entryAnchorOffset = reader.ReadUInt16();
                ushort exitAnchorOffset = reader.ReadUInt16();
                Records[i] = new GPOS.EntryExitRecord()
                {
                    EntryAnchor = new AnchorTable(reader, log, startPos + entryAnchorOffset),
                    ExitAnchor = new AnchorTable(reader, log, startPos + exitAnchorOffset),
                };
            }
        }
    }

    /// <summary>
    /// The MarkToBase attachment (MarkBasePos) subtable is used to position combining mark glyphs with respect to base glyphs. <para/>
    /// For example, the Arabic, Hebrew, and Thai scripts combine vowels, diacritical marks, and tone marks with base glyphs.<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/gpos#lookup-type-4-mark-to-base-attachment-positioning-subtable
    /// </summary>
    public class MarkToBaseAttachmentPosSubTable : GPosLookupSubTable
    {
        public CoverageTable MarkCoverage { get; internal set; }

        public CoverageTable BaseCoverage { get; internal set; }

        public MarkArrayTable MarkArray { get; internal set; }

        public BaseArrayTable BaseArray { get; internal set; }

        public ushort MarkClassCount { get; internal set; }

        protected override void OnRead(BinaryEndianAgnosticReader reader, Logger log, long startPos, ushort markFilteringSet, ushort posFormat)
        {
            switch (posFormat)
            {
                case 1:
                    ushort markCoverageOffset = reader.ReadUInt16();
                    ushort baseCoverageOffset = reader.ReadUInt16();
                    MarkClassCount = reader.ReadUInt16();
                    ushort markArrayOffset = reader.ReadUInt16();
                    ushort baseArrayOffset = reader.ReadUInt16();

                    MarkCoverage = new CoverageTable(reader, log, startPos + markCoverageOffset);
                    BaseCoverage = new CoverageTable(reader, log, startPos + baseCoverageOffset);
                    MarkArray = new MarkArrayTable(reader, log, startPos + markArrayOffset);
                    BaseArray = new BaseArrayTable(reader, log, startPos + baseArrayOffset, MarkClassCount);
                    break;
            }
        }
    }

    /// <summary>
    /// GPOS - Mark-to-Ligature Attachment Positioning Subtable.<para/>
    /// The MarkToLigature attachment (MarkLigPos) subtable is used to position combining mark glyphs with respect to ligature base glyphs. 
    /// With MarkToBase attachment, described previously, each base glyph has an attachment point defined for each class of marks. 
    /// MarkToLigature attachment is similar, except that each ligature glyph is defined to have multiple components (in a virtual sense — not actual glyphs), 
    /// and each component has a separate set of attachment points defined for the different mark classes.<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/gpos#lookup-type-5-mark-to-ligature-attachment-positioning-subtable
    /// </summary>
    public class MarkToLigatureAttachmentPosSubTable : GPosLookupSubTable
    {
        public CoverageTable MarkCoverage { get; internal set; }

        public CoverageTable LigatureCoverage { get; internal set; }

        public MarkArrayTable MarkArray { get; internal set; }

        public LigatureArrayTable LigatureArray { get; internal set; }

        public ushort MarkClassCount { get; internal set; }

        protected override void OnRead(BinaryEndianAgnosticReader reader, Logger log, long startPos, ushort markFilteringSet, ushort posFormat)
        {
            switch (posFormat)
            {
                case 1:
                    ushort markCoverageOffset = reader.ReadUInt16();
                    ushort ligatureCoverageOffset = reader.ReadUInt16();
                    ushort markClassCount = reader.ReadUInt16();
                    ushort markArrayOffset = reader.ReadUInt16();
                    ushort ligatureArrayOffset = reader.ReadUInt16();

                    MarkCoverage = new CoverageTable(reader, log, startPos + markCoverageOffset);
                    LigatureCoverage = new CoverageTable(reader, log, startPos + ligatureCoverageOffset);
                    MarkArray = new MarkArrayTable(reader, log, startPos + markArrayOffset);
                    LigatureArray = new LigatureArrayTable(reader, log, startPos + ligatureArrayOffset, MarkClassCount);
                    break;
            }
        }
    }

    /// <summary>
    /// GPOS - Mark-to-Mark attachment positoning table.<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/gpos#lookup-type-6-mark-to-mark-attachment-positioning-subtable
    /// </summary>
    public class MarkToMarkAttachmentPosSubTable : GPosLookupSubTable
    {
        public CoverageTable Mark1Coverage { get; internal set; }

        public CoverageTable Mark2Coverage { get; internal set; }

        public ushort MarkClassCount { get; internal set; }

        public MarkArrayTable Mark1Array { get; internal set; }

        /// <summary>
        /// Gets a <see cref="Mark2ArrayTable"/> containing records which each contain an array of <see cref="AnchorTable"/> instances (one per class).
        /// </summary>
        public Mark2ArrayTable Mark2Array { get; internal set; }

        protected override void OnRead(BinaryEndianAgnosticReader reader, Logger log, long startPos, ushort markFilteringSet, ushort posFormat)
        {
            ushort mark1CoverageOffset = reader.ReadUInt16();
            ushort mark2CoverageOffset = reader.ReadUInt16();
            MarkClassCount = reader.ReadUInt16();
            ushort mark1ArrayOffset = reader.ReadUInt16();
            ushort mark2ArrayOffset = reader.ReadUInt16();

            Mark1Coverage = new CoverageTable(reader, log, startPos + mark1CoverageOffset);
            Mark2Coverage = new CoverageTable(reader, log, startPos + mark2CoverageOffset);
            Mark1Array = new MarkArrayTable(reader, log, startPos + mark1ArrayOffset);
            Mark2Array = new Mark2ArrayTable(reader, log, startPos + mark2ArrayOffset, MarkClassCount);
        }
    }

    public class ContextPosSubTable : GPosLookupSubTable
    {
        public ClassDefinitionTable ClassDefinitions { get; internal set; }

        public CoverageTable[] Coverages { get; internal set; }

        public PosRuleSetTable[] RuleSetTables { get; internal set; }

        public PosClassSetTable[] ClassSets { get; internal set; }

        /// <summary>
        /// Gets an array of <see cref="PosLookupRecord"/> instances. Only present in format 3.
        /// </summary>
        public PosLookupRecord[] Records { get; internal set; }

        protected override void OnRead(BinaryEndianAgnosticReader reader, Logger log, long startPos, ushort markFilteringSet, ushort posFormat)
        {
            ushort coverageOffset;

            switch (posFormat)
            {
                case 1:
                    coverageOffset = reader.ReadUInt16();
                    ushort posRuleSetCount = reader.ReadUInt16();
                    ushort[] posRuleSetOffsets = reader.ReadArrayUInt16(posRuleSetCount);
                    Coverages = new CoverageTable[] { new CoverageTable(reader, log, startPos + coverageOffset) };
                    RuleSetTables = new PosRuleSetTable[posRuleSetCount];

                    for (int i = 0; i < posRuleSetCount; i++)
                        RuleSetTables[i] = new PosRuleSetTable(reader, log, startPos + posRuleSetOffsets[i]);

                    break;

                case 2:
                    coverageOffset = reader.ReadUInt16();
                    ushort classDefOffset = reader.ReadUInt16();
                    ushort posClassSetCount = reader.ReadUInt16();
                    ushort[] posClassSetOffsets = reader.ReadArrayUInt16(posClassSetCount);

                    Coverages = new CoverageTable[] { new CoverageTable(reader, log, startPos + coverageOffset) };
                    ClassDefinitions = new ClassDefinitionTable(reader, log, startPos + classDefOffset);
                    ClassSets = new PosClassSetTable[posClassSetCount];
                    for (int i = 0; i < posClassSetCount; i++)
                        ClassSets[i] = new PosClassSetTable(reader, log, startPos + posClassSetOffsets[i]);

                    break;

                case 3:
                    ushort glyphCount = reader.ReadUInt16();
                    ushort posCount = reader.ReadUInt16();
                    ushort[] coverageOffsets = reader.ReadArrayUInt16(glyphCount);

                    Records = new PosLookupRecord[posCount];
                    for(int i = 0; i < posCount; i++)
                    {
                        Records[i] = new PosLookupRecord()
                        {
                            SequenceIndex = reader.ReadUInt16(),
                            LookupListIndex = reader.ReadUInt16(),
                        };
                    }

                    Coverages = new CoverageTable[glyphCount];
                    for (int i = 0; i < glyphCount; i++)
                        Coverages[i] = new CoverageTable(reader, log, coverageOffsets[i]);
                    break;
            }
        }
    }

    public class ChainingContextualPosSubTable : GPosLookupSubTable
    {
        public CoverageTable Coverage { get; internal set; }

        public ChainPosRuleSetTable[] ChainRuleSets { get; internal set; }

        public ClassDefinitionTable BacktrackClasses { get; internal set; }

        public ClassDefinitionTable InputClasses { get; internal set; }

        public ClassDefinitionTable LookAheadClasses { get; internal set; }

        public PosLookupRecord[] Records { get; internal set; }

        protected override void OnRead(BinaryEndianAgnosticReader reader, Logger log, long startPos, ushort markFilteringSet, ushort posFormat)
        {
            ushort coverageOffset;

            switch (posFormat)
            {
                case 1: // ChainRuleSets contains glyph IDs.
                    coverageOffset = reader.ReadUInt16();
                    ushort chainPosRuleSetCount = reader.ReadUInt16();
                    ushort[] chainPosRuleSetOffsets = reader.ReadArrayUInt16(chainPosRuleSetCount);
                    ChainRuleSets = new ChainPosRuleSetTable[chainPosRuleSetCount];
                    for (int i = 0; i < chainPosRuleSetCount; i++)
                        ChainRuleSets[i] = new ChainPosRuleSetTable(reader, log, startPos + chainPosRuleSetOffsets[i]);

                    Coverage = new CoverageTable(reader, log, startPos + coverageOffset);
                    break;

                case 2: // ChainRuleSets contains class IDs instead of glyph IDs
                    coverageOffset = reader.ReadUInt16();
                    ushort backtrackClassDefOffset = reader.ReadUInt16();
                    ushort inputClassDefOffset = reader.ReadUInt16();
                    ushort lookAheadClassDefOffset = reader.ReadUInt16();
                    ushort chainPosClassSetCount = reader.ReadUInt16();
                    ushort[] chainPosClassSetOffsets = reader.ReadArrayUInt16(chainPosClassSetCount);

                    Coverage = new CoverageTable(reader, log, startPos + coverageOffset);
                    BacktrackClasses = new ClassDefinitionTable(reader, log, startPos + backtrackClassDefOffset);
                    InputClasses = new ClassDefinitionTable(reader, log, startPos + inputClassDefOffset);
                    LookAheadClasses = new ClassDefinitionTable(reader, log, startPos + lookAheadClassDefOffset);
                    ChainRuleSets = new ChainPosRuleSetTable[chainPosClassSetCount];
                    for (int i = 0; i < chainPosClassSetCount; i++)
                        ChainRuleSets[i] = new ChainPosRuleSetTable(reader, log, startPos + chainPosClassSetOffsets[i]);
                    break;

                case 3:
                    ushort backtrackGlyphCount = reader.ReadUInt16();
                    ushort[] backtrackCoverageOffsets = reader.ReadArrayUInt16(backtrackGlyphCount);

                    ushort inputGlyphCount = reader.ReadUInt16();
                    ushort[] inputCoverageOffsets = reader.ReadArrayUInt16(inputGlyphCount);

                    ushort lookAheadGlyphCount = reader.ReadUInt16();
                    ushort[] lookAheadCoverageOffsets = reader.ReadArrayUInt16(lookAheadGlyphCount);

                    ushort posCount = reader.ReadUInt16();
                    Records = new PosLookupRecord[posCount];
                    for(int i = 0; i < posCount; i++)
                    {
                        Records[i] = new PosLookupRecord()
                        {
                            SequenceIndex = reader.ReadUInt16(),
                            LookupListIndex = reader.ReadUInt16()
                        };
                    }
                    break;
            }
        }
    }
}
