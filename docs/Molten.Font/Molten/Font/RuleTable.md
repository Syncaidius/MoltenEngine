  
# Molten.Font.RuleTable

            A PosRule table also contains a count of the positioning operations to be performed on the input glyph sequence (posCount) and an array of PosLookupRecords (posLookupRecords). 
            Each record specifies a position in the input glyph sequence and a LookupList index to the positioning lookup to be applied there. 
            The array should list records in design order, or the order the lookups should be applied to the entire glyph sequence.<para />
            See for PosRuleTable: https://docs.microsoft.com/en-us/typography/opentype/spec/gpos#context-positioning-subtable-format-1-simple-glyph-contexts <para />
            See for SubRuleTable: https://docs.microsoft.com/en-us/typography/opentype/spec/gsub#51-context-substitution-format-1-simple-glyph-contexts
            
  
*  [InputSequence](docs/Molten.Font/Molten/Font/RuleTable/InputSequence.md)  
*  [Records](docs/Molten.Font/Molten/Font/RuleTable/Records.md)