  
# Molten.Font.SingleSubTable

            GSUB - Single Substitution table.<para />
            Format 1 calculates the indices of the output glyphs, which are not explicitly defined in the subtable. 
            To calculate an output glyph index, Format 1 adds a constant delta value to the input glyph index. 
            For the substitutions to occur properly, the glyph indices in the input and output ranges must be in the same order. 
            This format does not use the Coverage index that is returned from the Coverage table.<para />
            See: https://docs.microsoft.com/en-us/typography/opentype/spec/gsub#11-single-substitution-format-1
            
  
*  [Coverage](docs/Molten.Font/Molten/Font/SingleSubTable/Coverage.md)  
*  [DeltaGlyphID](docs/Molten.Font/Molten/Font/SingleSubTable/DeltaGlyphID.md)  
*  [SubstitudeGlyphIDs](docs/Molten.Font/Molten/Font/SingleSubTable/SubstitudeGlyphIDs.md)