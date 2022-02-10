using Molten.IO;

namespace Molten.Font
{
    /// <summary>Index-to-location table.<para/>
    /// <para>The indexToLoc table stores the offsets to the locations of the glyphs in the font, relative to the beginning of the glyphData table. In order to compute the length of the last glyph element, there is an extra entry after the last valid index.</para>
    /// <para>By definition, index zero points to the "missing character," which is the character that appears if a character is not found in the font. The missing character is commonly represented by a blank box or a space. If the font does not contain an outline for the missing character, then the first and second offsets should have the same value. This also applies to any other characters without an outline, such as the space character. If a glyph has no outline, then loca[n] = loca [n+1]. In the particular case of the last glyph(s), loca[n] will be equal the length of the glyph data ('glyf') table. The offsets must be in ascending order with loca[n] less-or-equal-to loca[n+1].</para>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/head </summary>
    [FontTableTag("OS/2")]
    public class Os2 : FontTable
    {
        public ushort Version { get; internal set; }

        /// <summary>
        /// Gets the Average Character Width parameter specifies the arithmetic average of the escapement (width) of all non-zero width glyphs in the font. 
        /// </summary>
        public short XAverageCharWidth { get; internal set; }

        /// <summary>
        /// Gets a value indicating the visual weight (degree of blackness or thickness of strokes) of the characters in the font. Values from 1 to 1000 are valid.
        /// </summary>
        public ushort UsWeightClass { get; internal set; }

        /// <summary>
        /// Gets the flags indicating a relative change from the normal aspect ratio (width to height ratio) as specified by a font designer for the glyphs in a font.
        /// </summary>
        public FontWidthClass WidthClass { get; internal set; }

        /// <summary>
        /// Gets the flags indicating the font's embedding licensing rights. Embeddable fonts may be stored in a document. 
        /// </summary>
        public FontEmbeddingFlags EmbeddingFlags { get; internal set; }

        /// <summary>
        /// Gets the recommended horizontal size in font design units for subscripts for this font.
        /// </summary>
        public short YSubscriptXSize { get; internal set; }

        /// <summary>
        /// Gets the recommended vertical size in font design units for subscripts for this font.
        /// </summary>
        public short YSubscriptYSize { get; internal set; }

        /// <summary>
        /// Gets the recommended horizontal offset in font design untis for subscripts for this font.
        /// </summary>
        public short YSubscriptXOffset { get; internal set; }

        /// <summary>
        /// Gets the recommended vertical offset in font design units from the baseline for subscripts for this font.
        /// </summary>
        public short YSubscriptYOffset { get; internal set; }

        /// <summary>
        /// Gets the recommended horizontal size in font design units for superscripts for this font.
        /// </summary>
        public short YSuperscriptXSize { get; internal set; }

        /// <summary>
        /// Gets the recommended vertical size in font design units for superscripts for this font.
        /// </summary>
        public short YSuperscriptYSize { get; internal set; }

        /// <summary>
        /// Gets the recommended horizontal offset in font design units for superscripts for this font.
        /// </summary>
        public short YSuperscriptXOffset { get; internal set; }

        /// <summary>
        /// Gets the recommended vertical offset in font design units from the baseline for superscripts for this font.
        /// </summary>
        public short YSuperscriptYOffset { get; internal set; }

        /// <summary>
        /// Gets the width of the strikeout stroke in font design units.
        /// </summary>
        public short YStrikeoutSize { get; internal set; }

        /// <summary>
        /// Gets the position of the top of the strikeout stroke relative to the baseline in font design units.
        /// </summary>
        public short YStrikeoutPosition { get; internal set; }

        /// <summary>
        /// Gets the classification of font-family design.
        /// </summary>
        public short SFamilyClass { get; internal set; }

        /// <summary>
        /// Gets values used to describe the visual characteristics of a given typeface. <para/>
        /// These characteristics are then used to associate the font with other fonts of similar appearance having different names. <para/>
        /// The variables for each digit are listed at: https://docs.microsoft.com/en-us/typography/opentype/spec/os2#panose. <para/>
        /// The Panose values are fully described in the Panose “greybook” reference, currently owned by Monotype Imaging.
        /// </summary>
        public byte[] Panose { get; internal set; }

        public uint UlUnicodeRange1 { get; internal set; }

        public uint UlUnicodeRange2 { get; internal set; }

        public uint UlUnicodeRange3 { get; internal set; }

        public uint UlUnicodeRange4 { get; internal set; }

        public byte[] AchVendID { get; internal set; }

        public FontSelectionFlags SelectionFlags { get; internal set; }

        public ushort UsFirstCharIndex { get; internal set; }

        public ushort UsLastCharIndex { get; internal set; }

        public short STypoAscender { get; internal set; }

        public short STypoDescender { get; internal set; }

        public short STypoLineGap { get; internal set; }

        public ushort UsWinAscent { get; internal set; }

        public ushort UsWinDescent { get; internal set; }

        public uint UlCodePageRange1 { get; internal set; }

        public uint UlCodePageRange2 { get; internal set; }

        public short SxHeight { get; internal set; }

        public short SCapHeight { get; internal set; }

        public ushort UsDefaultChar { get; internal set; }

        public ushort UsBreakChar { get; internal set; }

        public ushort UsMaxContext { get; internal set; }

        public ushort UsLowerOpticalPointSize { get; internal set; }

        public ushort UsUpperOpticalPointSize { get; internal set; }

        internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
        {
            // Read common table data. This is the same layout for all versions.
            Version = reader.ReadUInt16();
            XAverageCharWidth = reader.ReadInt16();
            UsWeightClass = reader.ReadUInt16();
            WidthClass = (FontWidthClass)reader.ReadUInt16();
            EmbeddingFlags = (FontEmbeddingFlags)reader.ReadUInt16();
            YSubscriptXSize = reader.ReadInt16();
            YSubscriptYSize = reader.ReadInt16();
            YSubscriptXOffset = reader.ReadInt16();
            YSubscriptYOffset = reader.ReadInt16();
            YSuperscriptXSize = reader.ReadInt16();
            YSuperscriptYSize = reader.ReadInt16();
            YSuperscriptXOffset = reader.ReadInt16();
            YSuperscriptYOffset = reader.ReadInt16();
            YStrikeoutSize = reader.ReadInt16();
            YStrikeoutPosition = reader.ReadInt16();
            SFamilyClass = reader.ReadInt16();
            Panose = reader.ReadBytes(10);
            UlUnicodeRange1 = reader.ReadUInt32();
            UlUnicodeRange2 = reader.ReadUInt32();
            UlUnicodeRange3 = reader.ReadUInt32();
            UlUnicodeRange4 = reader.ReadUInt32();
            AchVendID = reader.ReadBytes(4);
            SelectionFlags = (FontSelectionFlags)reader.ReadUInt16();
            UsFirstCharIndex = reader.ReadUInt16();
            UsLastCharIndex = reader.ReadUInt16();
            STypoAscender = reader.ReadInt16();
            STypoDescender = reader.ReadInt16();
            STypoLineGap = reader.ReadInt16();
            UsWinAscent = reader.ReadUInt16();
            UsWinDescent = reader.ReadUInt16();

            if (Version >= 1)
            {
                UlCodePageRange1 = reader.ReadUInt32();
                UlCodePageRange2 = reader.ReadUInt32();
            }

            if (Version >= 2)
            {
                SxHeight = reader.ReadInt16();
                SCapHeight = reader.ReadInt16();
                UsDefaultChar = reader.ReadUInt16();
                UsBreakChar = reader.ReadUInt16();
            }

            if (Version >= 3)
                UsMaxContext = reader.ReadUInt16();

            if (Version >= 5)
            {
                UsLowerOpticalPointSize = reader.ReadUInt16();
                UsUpperOpticalPointSize = reader.ReadUInt16();
            }
        }
    }
}
