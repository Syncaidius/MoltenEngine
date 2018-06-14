  
# Molten.Font.RatioRangeRecord

            Represents an aspect ratio range.<para />
            All values set to zero signal the default grouping to use; if present, this must be the last Ratio group in the table. Ratios of 2:2 are the same as 1:1.
            <para>
            Ratios are set up as follows: <para />
            For a 1:1 aspect ratio  Ratios.xRatio = 1; Ratios.yStartRatio = 1; Ratios.yEndRatio = 1; <para />
            For 1:1 through 2:1 ratio Ratios.xRatio = 2; Ratios.yStartRatio = 1; Ratios.yEndRatio = 2; <para />
            For 1.33:1 ratio Ratios.xRatio = 4; Ratios.yStartRatio = 3; Ratios.yEndRatio = 3; <para />
            For _all_ aspect ratios Ratio.xRatio = 0; Ratio.yStartRatio = 0; Ratio.yEndRatio = 0; <para /></para>
  
*  [BCharSet](docs/Molten.Font/Molten/Font/RatioRangeRecord/BCharSet.md)  
*  [XRatio](docs/Molten.Font/Molten/Font/RatioRangeRecord/XRatio.md)  
*  [YEndRatio](docs/Molten.Font/Molten/Font/RatioRangeRecord/YEndRatio.md)  
*  [YStartRatio](docs/Molten.Font/Molten/Font/RatioRangeRecord/YStartRatio.md)