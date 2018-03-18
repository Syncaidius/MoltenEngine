using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Control value program table .<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/prep </summary>
    [FontTableTag("GSUB")]
    public class GSUB : FontGTable
    {
        protected override ushort GetExtensionIndex()
        {
            return (ushort)GSUBLookupType.ExtensionSubstitution;
        }

        protected override Type[] GetLookupTypeIndex()
        {
            return new Type[]
            {
                null, // Type 0,
                typeof(SingleSubTable), // Type 1
                typeof(MultipleSubTable), // Type 2
                typeof(AlternateSubTable), // Type 3
                typeof(LigatureSubTable), // Type 4
                typeof(ContextualSubTable), // Type 5
                typeof(ChainingContextualSubTable), // Type 6
                null, // Type 7 - extension lookup
                typeof(ReverseChainingContextualSingleSubTable) // Type 8
            };
        }
    }

    public enum GSUBLookupType : ushort
    {
        /// <summary>No lookup. Invalid.</summary>
        None = 0,

        /// <summary>
        /// Replace one glyph with one glyph
        /// </summary>
        Single = 1,

        /// <summary>
        /// Replace one glyph with more than one glyph
        /// </summary>
        Multiple = 2,

        /// <summary>
        /// Replace one glyph with one of many glyphs
        /// </summary>
        Alternate = 3,

        /// <summary>
        /// Replace multiple glyphs with one glyph
        /// </summary>
        Ligature = 4,

        /// <summary>
        /// Replace one or more glyphs in context
        /// </summary>
        Context = 5,

        /// <summary>
        /// Replace one or more glyphs in chained context
        /// </summary>
        ChainingContext = 6,

        /// <summary>
        /// Extension mechanism for other substitutions (i.e. this excludes the Extension type substitution itself)
        /// </summary>
        ExtensionSubstitution = 7,

        /// <summary>
        /// Applied in reverse order, replace single glyph in chaining context
        /// </summary>
        ReverseChainingContextSingle = 8,

        Reserved = 9,
    }

    /// <summary>
    /// A base class for all GPOS lookup tables.
    /// </summary>
    public abstract class GSubLookupSubTable : LookupTable
    {
        public GSUBLookupType Type { get; protected set; }

        public ushort Format { get; private set; }

        internal GSubLookupSubTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset,
            ushort lookupType, LookupFlags flags, ushort markFilteringSet) :
            base(reader, log, parent, offset, lookupType, flags, markFilteringSet)
        {
            Type = (GSUBLookupType)lookupType;
            Format = reader.ReadUInt16();
        }
    }

    /// <summary>
    /// GSUB - Single Substitution table.<para/>
    /// Format 1 calculates the indices of the output glyphs, which are not explicitly defined in the subtable. 
    /// To calculate an output glyph index, Format 1 adds a constant delta value to the input glyph index. 
    /// For the substitutions to occur properly, the glyph indices in the input and output ranges must be in the same order. 
    /// This format does not use the Coverage index that is returned from the Coverage table.<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/gsub#11-single-substitution-format-1
    /// </summary>
    public class SingleSubTable : GSubLookupSubTable
    {
        /// <summary>
        /// Gets the delta value added to the original glyph ID to get substitute glyph ID.
        /// </summary>
        public short DeltaGlyphID { get; private set; }

        /// <summary>
        /// Gets a <see cref="CoverageTable"/> containing a list of glyphIDs to be substituted.
        /// </summary>
        public CoverageTable Coverage { get; private set; }

        /// <summary>
        /// Gets an array of substitute glyph IDs — ordered by Coverage index. <para/>
        /// Only present if <see cref="Format"/> is 2.
        /// </summary>
        public ushort[] SubstitudeGlyphIDs { get; private set; }

        internal SingleSubTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, 
            ushort lookupType, LookupFlags flags, ushort markFilteringSet) : 
            base(reader, log, parent, offset, lookupType, flags, markFilteringSet)
        {
            ushort coverageOffset = reader.ReadUInt16();

            switch (Format)
            {
                case 1:
                    DeltaGlyphID = reader.ReadInt16();
                    break;

                case 2:
                    ushort glyphCount = reader.ReadUInt16();
                    SubstitudeGlyphIDs = reader.ReadArray<ushort>(glyphCount);
                    break;
            }

            Coverage = new CoverageTable(reader, log, this, coverageOffset);
        }
    }

    /// <summary>
    /// GSUB - Multiple substitution table. <para/>
    /// A Multiple Substitution (MultipleSubst) subtable replaces a single glyph with more than one glyph, as when multiple glyphs replace a single ligature.
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/gsub#lookuptype-2-multiple-substitution-subtable
    /// </summary>
    public class MultipleSubTable : GSubLookupSubTable
    {
        /// <summary>
        /// Gets an array of <see cref="SequenceTable"/>, ordered by Coverage index
        /// </summary>
        public SequenceTable[] Tables { get; private set; }

        public CoverageTable Coverage { get; private set; }

        internal MultipleSubTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, 
            ushort lookupType, LookupFlags flags, ushort markFilteringSet) : 
            base(reader, log, parent, offset, lookupType, flags, markFilteringSet)
        {
            switch (Format)
            {
                case 1:
                    ushort coverageOffset = reader.ReadUInt16();
                    ushort sequenceCount = reader.ReadUInt16();
                    Tables = new SequenceTable[sequenceCount];
                    ushort[] sequenceOffsets = reader.ReadArray<ushort>(sequenceCount);
                    for (int i = 0; i < sequenceCount; i++)
                        Tables[i] = new SequenceTable(reader, log, this, sequenceOffsets[i]);

                    Coverage = new CoverageTable(reader, log, this, coverageOffset);
                    break;

                default:
                    log.WriteWarning("Unsupported MultipleSubstitutionTable format");
                    break;
            }
        }
    }

    /// <summary>
    /// GSUB - Alternate substitution table. <para/>
    /// An Alternate Substitution (AlternateSubst) subtable identifies any number of aesthetic alternatives from which a user can choose a glyph variant to replace the input glyph. 
    /// For example, if a font contains four variants of the ampersand symbol, the cmap table will specify the index of one of the four glyphs as the default glyph index, 
    /// and an AlternateSubst subtable will list the indices of the other three glyphs as alternatives. 
    /// A text-processing client would then have the option of replacing the default glyph with any of the three alternatives.<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/gsub#lookuptype-3-alternate-substitution-subtable
    /// </summary>
    public class AlternateSubTable : GSubLookupSubTable
    {
        public AlternateSetTable[] Tables { get; private set; }

        public CoverageTable Coverage { get; private set; }

        internal AlternateSubTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset,
            ushort lookupType, LookupFlags flags, ushort markFilteringSet) :
            base(reader, log, parent, offset, lookupType, flags, markFilteringSet)
        {
            // Despite this format being identical to the MultipleSubstitutionTable, lets keep the implementation separate for clarity.
            switch (Format)
            {
                case 1:
                    ushort coverageOffset = reader.ReadUInt16();
                    ushort alternateSetCount = reader.ReadUInt16();
                    Tables = new AlternateSetTable[alternateSetCount];
                    ushort[] alternateSetOffsets = reader.ReadArray<ushort>(alternateSetCount);
                    for (int i = 0; i < alternateSetCount; i++)
                        Tables[i] = new AlternateSetTable(reader, log, this, alternateSetOffsets[i]);

                    Coverage = new CoverageTable(reader, log, this, coverageOffset);
                    break;
            }
        }
    }

    /// <summary>
    /// GSUB - Ligature Substitution Subtable. <para/>
    /// A Ligature Substitution (LigatureSubst) subtable identifies ligature substitutions where a single glyph replaces multiple glyphs. 
    /// One LigatureSubst subtable can specify any number of ligature substitutions. <para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/gsub#lookuptype-4-ligature-substitution-subtable
    /// </summary>
    public class LigatureSubTable : GSubLookupSubTable
    {
        public LigatureSetTable[] Tables { get; private set; }

        internal LigatureSubTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, 
            ushort lookupType, LookupFlags flags, ushort markFilteringSet) : 
            base(reader, log, parent, offset, lookupType, flags, markFilteringSet)
        {
            switch (Format)
            {
                case 1:
                    ushort coverageOffset = reader.ReadUInt16();
                    ushort ligatureSetCount = reader.ReadUInt16();
                    ushort[] ligatureSetOffsets = reader.ReadArray<ushort>(ligatureSetCount);
                    Tables = new LigatureSetTable[ligatureSetCount];

                    for (int i = 0; i < ligatureSetCount; i++)
                        Tables[i] = new LigatureSetTable(reader, log, this, ligatureSetOffsets[i]);
                    break;
            }
        }
    }

    /// <summary>
    /// GSUB - Contextual Substitution sub-table. <para/>
    /// A Contextual Substitution (ContextSubst) subtable defines the most powerful type of glyph substitution lookup: 
    /// it describes glyph substitutions in context that replace one or more glyphs within a certain pattern of glyphs.<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/gsub#lookuptype-5-contextual-substitution-subtable
    /// </summary>
    public class ContextualSubTable : GSubLookupSubTable
    {
        /// <summary>
        /// Gets an array of subsitition rule set tables.
        /// </summary>
        public RuleSetTable[] RuleSets { get; private set; }

        /// <summary>
        /// Gets an array of coverage tables.
        /// </summary>
        public CoverageTable[] Coverages { get; private set; }

        /// <summary>
        /// Gets an array of offsets to subsitition class set tables, ordered by class (may be null).
        /// </summary>
        public ClassSetTable[] ClassSets { get; private set; }

        /// <summary>
        /// Gets an array of sustitution lookup records, in design order.
        /// </summary>
        public RuleLookupRecord[] Records { get; private set; }

        internal ContextualSubTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, 
            ushort lookupType, LookupFlags flags, ushort markFilteringSet) : 
            base(reader, log, parent, offset, lookupType, flags, markFilteringSet)
        {
            ushort coverageOffset;

            switch (Format)
            {
                case 1:
                    coverageOffset = reader.ReadUInt16();
                    ushort subRuleSetCount = reader.ReadUInt16();
                    ushort[] subRuleSetOffsets = reader.ReadArray<ushort>(subRuleSetCount);
                    RuleSets = new RuleSetTable[subRuleSetCount];
                    for (int i = 0; i < subRuleSetCount; i++)
                        RuleSets[i] = new RuleSetTable(reader, log, this, subRuleSetOffsets[i]);

                    Coverages = new CoverageTable[1];
                    Coverages[0] = new CoverageTable(reader, log, this, coverageOffset);
                    break;

                case 2:
                    coverageOffset = reader.ReadUInt16();
                    ushort classDefOffset = reader.ReadUInt16();
                    ushort subClassSetCount = reader.ReadUInt16();
                    ushort[] subClassSetOffsets = reader.ReadArray<ushort>(subClassSetCount);
                    ClassSets = new ClassSetTable[subClassSetCount];
                    for (int i = 0; i < subClassSetCount; i++)
                    {
                        if(subClassSetOffsets[i] > FontUtil.NULL)
                            ClassSets[i] = new ClassSetTable(reader, log, this, subClassSetOffsets[i]);
                    }

                    Coverages = new CoverageTable[1];
                    Coverages[0] = new CoverageTable(reader, log, this, coverageOffset);
                    break;

                case 3:
                    ushort glyphCount = reader.ReadUInt16();
                    ushort substitutionCount = reader.ReadUInt16();
                    ushort[] coverageOffsets = reader.ReadArray<ushort>(glyphCount);

                    Records = new RuleLookupRecord[substitutionCount];
                    Coverages = new CoverageTable[glyphCount];
                    for (int i = 0; i < substitutionCount; i++)
                    {
                        Records[i] = new RuleLookupRecord()
                        {
                            SequenceIndex = reader.ReadUInt16(),
                            LookupListIndex = reader.ReadUInt16(),
                        };
                    }

                    for (int i = 0; i < glyphCount; i++)
                        Coverages[i] = new CoverageTable(reader, log, this, coverageOffsets[i]);
                    break;
            }
        }
    }

    /// <summary>
    /// GSUB - Chaining contextual substitution table. <para/>
    /// This lookup provides a mechanism whereby any other lookup type's subtables are stored at a 32-bit offset location in the 'GSUB' table. 
    /// This is needed if the total size of the subtables exceeds the 16-bit limits of the various other offsets in the 'GSUB' table. In this specification,
    /// the subtable stored at the 32-bit offset location is termed the “extension” subtable. <para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/gsub#lookuptype-6-chaining-contextual-substitution-subtable
    /// </summary>
    public class ChainingContextualSubTable : GSubLookupSubTable
    {
        public CoverageTable Coverage { get; internal set; }

        public ChainRuleSetTable[] ChainRuleSets { get; internal set; }

        public ClassDefinitionTable BacktrackClasses { get; internal set; }

        public ClassDefinitionTable InputClasses { get; internal set; }

        public ClassDefinitionTable LookAheadClasses { get; internal set; }

        public RuleLookupRecord[] Records { get; internal set; }

        internal ChainingContextualSubTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset,
            ushort lookupType, LookupFlags flags, ushort markFilteringSet) :
            base(reader, log, parent, offset, lookupType, flags, markFilteringSet)
        {
            ushort coverageOffset;
            Dictionary<long, ChainRuleTable> existingRules = new Dictionary<long, ChainRuleTable>();

            switch (Format)
            {
                case 1: // ChainRuleSets contains glyph IDs.
                    coverageOffset = reader.ReadUInt16();
                    ushort chainPosRuleSetCount = reader.ReadUInt16();
                    ushort[] chainPosRuleSetOffsets = reader.ReadArray<ushort>(chainPosRuleSetCount);
                    ChainRuleSets = new ChainRuleSetTable[chainPosRuleSetCount];
                    for (int i = 0; i < chainPosRuleSetCount; i++)
                        ChainRuleSets[i] = new ChainRuleSetTable(reader, log, this, chainPosRuleSetOffsets[i], existingRules);

                    Coverage = new CoverageTable(reader, log, this, coverageOffset);
                    break;

                case 2: // ChainRuleSets contains class IDs instead of glyph IDs
                    coverageOffset = reader.ReadUInt16();
                    ushort backtrackClassDefOffset = reader.ReadUInt16();
                    ushort inputClassDefOffset = reader.ReadUInt16();
                    ushort lookAheadClassDefOffset = reader.ReadUInt16();
                    ushort chainPosClassSetCount = reader.ReadUInt16();
                    ushort[] chainPosClassSetOffsets = reader.ReadArray<ushort>(chainPosClassSetCount);

                    Coverage = new CoverageTable(reader, log, this, coverageOffset);
                    BacktrackClasses = new ClassDefinitionTable(reader, log, this, backtrackClassDefOffset);
                    InputClasses = new ClassDefinitionTable(reader, log, this, inputClassDefOffset);
                    LookAheadClasses = new ClassDefinitionTable(reader, log, this, lookAheadClassDefOffset);
                    ChainRuleSets = new ChainRuleSetTable[chainPosClassSetCount];
                    for (int i = 0; i < chainPosClassSetCount; i++)
                        ChainRuleSets[i] = new ChainRuleSetTable(reader, log, this, chainPosClassSetOffsets[i], existingRules);
                    break;

                case 3:
                    ushort backtrackGlyphCount = reader.ReadUInt16();
                    ushort[] backtrackCoverageOffsets = reader.ReadArray<ushort>(backtrackGlyphCount);

                    ushort inputGlyphCount = reader.ReadUInt16();
                    ushort[] inputCoverageOffsets = reader.ReadArray<ushort>(inputGlyphCount);

                    ushort lookAheadGlyphCount = reader.ReadUInt16();
                    ushort[] lookAheadCoverageOffsets = reader.ReadArray<ushort>(lookAheadGlyphCount);

                    ushort posCount = reader.ReadUInt16();
                    Records = new RuleLookupRecord[posCount];
                    for (int i = 0; i < posCount; i++)
                    {
                        Records[i] = new RuleLookupRecord()
                        {
                            SequenceIndex = reader.ReadUInt16(),
                            LookupListIndex = reader.ReadUInt16()
                        };
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// GSUB - Reverse chaining contextual single substitution table. <para/>
    /// This table describes single-glyph substitutions in context with an ability to look back and/or look ahead in the sequence of glyphs. 
    /// The major difference between this and other lookup types is that processing of input glyph sequence goes from end to start.<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/gsub#lookuptype-8-reverse-chaining-contextual-single-substitution-subtable
    /// </summary>
    public class ReverseChainingContextualSingleSubTable : GSubLookupSubTable
    {
        public CoverageTable Coverage { get; private set; }

        public CoverageTable[] BacktrackCoverages { get; private set; }

        public CoverageTable[] LookAheadCoverages { get; private set; }

        public ushort[] GlyphIDs { get; private set; }

        internal ReverseChainingContextualSingleSubTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset,
            ushort lookupType, LookupFlags flags, ushort markFilteringSet) :
            base(reader, log, parent, offset, lookupType, flags, markFilteringSet)
        {
            switch (Format)
            {
                case 1:
                    ushort coverageOffset = reader.ReadUInt16();
                    ushort backtrackGlyphCount = reader.ReadUInt16();
                    ushort[] backtrackCoverageOffsets = reader.ReadArray<ushort>(backtrackGlyphCount);
                    ushort lookAheadGlyphCount = reader.ReadUInt16();
                    ushort[] lookAheadCoverageOffsets = reader.ReadArray<ushort>(lookAheadGlyphCount);
                    ushort glyphCount = reader.ReadUInt16();
                    GlyphIDs = reader.ReadArray<ushort>(glyphCount);

                    Coverage = new CoverageTable(reader, log, this, coverageOffset);

                    BacktrackCoverages = new CoverageTable[backtrackGlyphCount];
                    for (int i = 0; i < backtrackGlyphCount; i++)
                        BacktrackCoverages[i] = new CoverageTable(reader, log, this, backtrackCoverageOffsets[i]);

                    LookAheadCoverages = new CoverageTable[lookAheadGlyphCount];
                    for (int i = 0; i < lookAheadGlyphCount; i++)
                        LookAheadCoverages[i] = new CoverageTable(reader, log, this, lookAheadCoverageOffsets[i]);
                    break;
            }
        }
    }
}
