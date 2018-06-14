  
# Molten.Font.Os2
Index-to-location table.<para /><para>The indexToLoc table stores the offsets to the locations of the glyphs in the font, relative to the beginning of the glyphData table. In order to compute the length of the last glyph element, there is an extra entry after the last valid index.</para><para>By definition, index zero points to the "missing character," which is the character that appears if a character is not found in the font. The missing character is commonly represented by a blank box or a space. If the font does not contain an outline for the missing character, then the first and second offsets should have the same value. This also applies to any other characters without an outline, such as the space character. If a glyph has no outline, then loca[n] = loca [n+1]. In the particular case of the last glyph(s), loca[n] will be equal the length of the glyph data ('glyf') table. The offsets must be in ascending order with loca[n] less-or-equal-to loca[n+1].</para>
            See: https://docs.microsoft.com/en-us/typography/opentype/spec/head 
  
*  [EmbeddingFlags](docs/Molten.Font/Molten/Font/Os2/EmbeddingFlags.md)  
*  [Panose](docs/Molten.Font/Molten/Font/Os2/Panose.md)  
*  [SFamilyClass](docs/Molten.Font/Molten/Font/Os2/SFamilyClass.md)  
*  [UsWeightClass](docs/Molten.Font/Molten/Font/Os2/UsWeightClass.md)  
*  [WidthClass](docs/Molten.Font/Molten/Font/Os2/WidthClass.md)  
*  [XAverageCharWidth](docs/Molten.Font/Molten/Font/Os2/XAverageCharWidth.md)  
*  [YStrikeoutPosition](docs/Molten.Font/Molten/Font/Os2/YStrikeoutPosition.md)  
*  [YStrikeoutSize](docs/Molten.Font/Molten/Font/Os2/YStrikeoutSize.md)  
*  [YSubscriptXOffset](docs/Molten.Font/Molten/Font/Os2/YSubscriptXOffset.md)  
*  [YSubscriptXSize](docs/Molten.Font/Molten/Font/Os2/YSubscriptXSize.md)  
*  [YSubscriptYOffset](docs/Molten.Font/Molten/Font/Os2/YSubscriptYOffset.md)  
*  [YSubscriptYSize](docs/Molten.Font/Molten/Font/Os2/YSubscriptYSize.md)  
*  [YSuperscriptXOffset](docs/Molten.Font/Molten/Font/Os2/YSuperscriptXOffset.md)  
*  [YSuperscriptXSize](docs/Molten.Font/Molten/Font/Os2/YSuperscriptXSize.md)  
*  [YSuperscriptYOffset](docs/Molten.Font/Molten/Font/Os2/YSuperscriptYOffset.md)  
*  [YSuperscriptYSize](docs/Molten.Font/Molten/Font/Os2/YSuperscriptYSize.md)