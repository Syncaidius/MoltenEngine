  
# Molten.Font.ChainingContextualSubTable

            GSUB - Chaining contextual substitution table. <para />
            This lookup provides a mechanism whereby any other lookup type's subtables are stored at a 32-bit offset location in the 'GSUB' table. 
            This is needed if the total size of the subtables exceeds the 16-bit limits of the various other offsets in the 'GSUB' table. In this specification,
            the subtable stored at the 32-bit offset location is termed the “extension” subtable. <para />
            See: https://docs.microsoft.com/en-us/typography/opentype/spec/gsub#lookuptype-6-chaining-contextual-substitution-subtable
            
