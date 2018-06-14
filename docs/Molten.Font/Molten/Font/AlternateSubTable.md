  
# Molten.Font.AlternateSubTable

            GSUB - Alternate substitution table. <para />
            An Alternate Substitution (AlternateSubst) subtable identifies any number of aesthetic alternatives from which a user can choose a glyph variant to replace the input glyph. 
            For example, if a font contains four variants of the ampersand symbol, the cmap table will specify the index of one of the four glyphs as the default glyph index, 
            and an AlternateSubst subtable will list the indices of the other three glyphs as alternatives. 
            A text-processing client would then have the option of replacing the default glyph with any of the three alternatives.<para />
            See: https://docs.microsoft.com/en-us/typography/opentype/spec/gsub#lookuptype-3-alternate-substitution-subtable
            
