using System.Collections.Generic;

namespace Molten.Font
{
    /// <summary>
    /// Character map table format 14. <para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/cmap#format-14-unicode-variation-sequences
    /// </summary>
    public class CmapFormat14SubTable : CmapSubTable
    {
        Dictionary<uint, VariationSelector> _varSelectors;

        internal CmapFormat14SubTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, CmapEncodingRecord record) :
            base(reader, log, parent, offset, record)
        {
            uint length = reader.ReadUInt32();
            uint numVarSelectorRecords = reader.ReadUInt32();
            uint[] varSelectors = new uint[numVarSelectorRecords];
            uint[] defaultUvsOffsets = new uint[numVarSelectorRecords];
            uint[] nonDefaultUvsOffsets = new uint[numVarSelectorRecords];

            for (int i = 0; i < numVarSelectorRecords; i++)
            {
                varSelectors[i] = reader.ReadUInt24();
                defaultUvsOffsets[i] = reader.ReadUInt32();
                nonDefaultUvsOffsets[i] = reader.ReadUInt32();
            }

            // Populate variation selectors
            _varSelectors = new Dictionary<uint, VariationSelector>();
            for (int i = 0; i < numVarSelectorRecords; i++)
            {
                VariationSelector selector = new VariationSelector();
                if (!FontUtil.IsNull(defaultUvsOffsets[i]))
                    new DefaultUvsTable(reader, log, this, defaultUvsOffsets[i], selector);

                if (!FontUtil.IsNull(nonDefaultUvsOffsets[i]))
                    new NonDefaultUvsTable(reader, log, this, nonDefaultUvsOffsets[i], selector);

                _varSelectors.Add(varSelectors[i], selector);
            }
        }

        public override ushort CharPairToGlyphIndex(uint codepoint, ushort defaultGlyphIndex, uint nextCodepoint)
        {
            // Only check codepoint if nextCodepoint is a variation selector
            VariationSelector sel;
            if (_varSelectors.TryGetValue(nextCodepoint, out sel))
            {
                // If the sequence is a non-default UVS, return the mapped glyph
                ushort ret = 0;
                if (sel.UVSMappings.TryGetValue(codepoint, out ret))
                {
                    return ret;
                }

                // If the sequence is a default UVS, return the default glyph
                for (int i = 0; i < sel.DefaultStartCodes.Count; ++i)
                {
                    if (codepoint >= sel.DefaultStartCodes[i] && codepoint < sel.DefaultEndCodes[i])
                    {
                        return defaultGlyphIndex;
                    }
                }

                // At this point we are neither a non-default UVS nor a default UVS,
                // but we know the nextCodepoint is a variation selector. Unicode says
                // this glyph should be invisible: “no visible rendering for the VS”
                // (http://unicode.org/faq/unsup_char.html#4)
                return defaultGlyphIndex;
            }

            // In all other cases, return 0
            return 0;
        }

        public override ushort CharToGlyphIndex(uint codepoint)
        {
            return 0;
        }
    }

    internal class VariationSelector
    {
        public List<uint> DefaultStartCodes = new List<uint>();
        public List<uint> DefaultEndCodes = new List<uint>();
        public Dictionary<uint, ushort> UVSMappings = new Dictionary<uint, ushort>();
    }

    internal class DefaultUvsTable : FontSubTable
    {
        internal DefaultUvsTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, VariationSelector varSelector) :
            base(reader, log, parent, offset)
        {
            uint numUnicodeValueRanges = reader.ReadUInt32();
            for (int i = 0; i < numUnicodeValueRanges; i++)
            {
                uint startCode = reader.ReadUInt24();
                byte additionalCount = reader.ReadByte();
                uint endCode = startCode + additionalCount;

                varSelector.DefaultStartCodes.Add(startCode);
                varSelector.DefaultEndCodes.Add(endCode);
            }
        }
    }

    internal class NonDefaultUvsTable : FontSubTable
    {
        internal NonDefaultUvsTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, VariationSelector varSelector) :
            base(reader, log, parent, offset)
        {
            uint numUvsMappings = reader.ReadUInt32();
            for (int n = 0; n < numUvsMappings; ++n)
            {
                uint unicodeValue = reader.ReadUInt24();
                ushort glyphID = reader.ReadUInt16();
                varSelector.UVSMappings.Add(unicodeValue, glyphID);
            }
        }
    }
}
