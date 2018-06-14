  
# Molten.Font.VDMX
VDMX - Vertical Device Metrics table; Relates to OpenType™ fonts with TrueType outlines ines. <para />
            Under Windows, the usWinAscent and usWinDescent values from the 'OS/2' table will be used to determine the maximum black height for a font at any given size. <para />
            Windows calls this distance the Font Height. Because TrueType instructions can lead to Font Heights that differ from the actual scaled and rounded values, basing the Font Height strictly on the yMax and yMin can result in “lost pixels.” 
            Windows will clip any pixels that extend above the yMax or below the yMin. In order to avoid grid fitting the entire font to determine the correct height, the VDMX table has been defined.<para />
            See: https://docs.microsoft.com/en-us/typography/opentype/spec/vdmx 
