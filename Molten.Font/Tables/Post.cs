using System.Collections.Generic;
using Molten.IO;

namespace Molten.Font
{
    /// <summary>PostScript table.<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/post </summary>
    [FontTableTag("post")]
    public class Post : FontTable
    {
        public ushort MajorVersion { get; private set; }

        public ushort MinorVersion { get; private set; }

        /// <summary>Gets the Italic angle in counter-clockwise degrees from the vertical. Zero for upright text, negative for text that leans to the right (forward).</summary>
        public double ItalicAngle { get; private set; }

        /// <summary>Gets the suggested distance of the top of the underline from the baseline, in font design units. Negative values indicate below baseline.<para/>
        /// The PostScript definition of this FontInfo dictionary key(the y coordinate of the center of the stroke) is not used for historical reasons.<para/>
        /// The value of the PostScript key may be calculated by subtracting half the underlineThickness from the value of this field.</summary>
        public short UnderlinePosition { get; private set; }

        /// <summary>Gets the suggested values for the underline thickness, in font design units.</summary>
        public short UnderlineThickness { get; private set; }

        /// <summary>Gets whether or not the font is proportionally spaced. False if the font is not proportionally spaced (i.e. monospaced).</summary>
        public bool IsFixedPitch { get; private set; }

        public uint MinMemoryType42 { get; private set; }

        public uint MaxMemoryType42 { get; private set; }

        public uint MinMemoryType1 { get; private set; }

        public uint MaxMemoryType1 { get; private set; }

        public ushort[] NameIndices { get; private set; }

        public string[] Names { get; private set; }

        /// <summary>Gets an array containing macintosh-standard gylph ID offsets. <para/>
        /// This is present in table version 2.5 when a font contains Macintosh-standard glyphs that have been reordered.</summary>
        public sbyte[] StandardOffsets { get; private set; }

        /// <summary>Returns either the macintosh standard glyph name or the extended name provided by the font.<para />
        /// This only works if the table is version 2.0 or 2.5.</summary>
        /// <param name="glyphID"></param>
        /// <returns></returns>
        public string GetGlyphName(int glyphID)
        {
            if (MajorVersion > 2)
            {
                if (MinorVersion == 0)
                {
                    // If the name index is between 258 and 32767, then subtract 258 and use that to index into the Names array.
                    if (glyphID > 257)
                        return Names[glyphID - 257];
                    else
                        return FontLookup.MacintoshGlyphNames[glyphID];
                }
                else if (MinorVersion == 5)
                {
                    int offsetID = glyphID + StandardOffsets[glyphID];
                    return FontLookup.MacintoshGlyphNames[offsetID];
                }
            }

            return string.Empty;
        }

        internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
        {
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
            ItalicAngle = FontUtil.FixedToDouble(reader.ReadInt32());
            UnderlinePosition = reader.ReadInt16();
            UnderlineThickness = reader.ReadInt16();
            IsFixedPitch = reader.ReadUInt32() == 0; // 0 if the font is proportionally spaced (fixed)
            MinMemoryType42 = reader.ReadUInt32();
            MaxMemoryType42 = reader.ReadUInt32();
            MinMemoryType1 = reader.ReadUInt32();
            MaxMemoryType1 = reader.ReadUInt32();

            switch (MajorVersion)
            {
                case 2 when MinorVersion == 0:
                    ushort numGlyphs = reader.ReadUInt16();
                    NameIndices = new ushort[numGlyphs];

                    // Use a hashset to track the number of non-Macintosh-standard glyphs.
                    HashSet<ushort> newGlyphs = new HashSet<ushort>();

                    // Read name indices
                    for (int i = 0; i < numGlyphs; i++)
                    {
                        ushort gID = reader.ReadUInt16();
                        NameIndices[i] = gID;

                        // If the name index is between 258 and 32767, then subtract 258 and use that to index into the list of Pascal strings at the end of the table.
                        if (gID > 257)
                            newGlyphs.Add(gID);
                    }

                    // Read new glyph names (stored in file as pascal strings).
                    int numNewGlyphs = newGlyphs.Count;
                    Names = new string[numNewGlyphs];
                    for (int i = 0; i < numNewGlyphs; i++)
                        Names[i] = reader.ReadPascalString();
                    break;

                case 2 when MinorVersion == 5:
                    ushort glyphCount = reader.ReadUInt16();
                    StandardOffsets = reader.ReadArray<sbyte>(glyphCount);
                    break;

                case 4 when MinorVersion == 0:
                    log.WriteDebugLine($"[post] Table format 4.{MinorVersion} (AAT) detected. Ignoring extra data.");
                    // From Apple docs: "As a rule, format 4 'post' tables are no longer necessary and should be avoided."
                    break;
            }
        }
    }

}
