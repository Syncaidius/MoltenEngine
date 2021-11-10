namespace Molten.Font
{
    public enum CaretValueFormat : ushort
    {
        Invalid = 0,

        /// <summary>Consists of a format identifier (CaretValueFormat), followed by a single coordinate for the caret position (Coordinate). 
        /// The Coordinate is in design units. This format has the benefits of small size and simplicity, 
        /// but the Coordinate value cannot be hinted for fine adjustments at different device resolutions.</summary>
        DesignUnits = 1,

        /// <summary>Specifies the caret coordinate in terms of a contour point index on a specific glyph. During font hinting, 
        /// the contour point on the glyph outline may move. <para />
        /// The point's final position after hinting provides the final value for rendering a given font size. 
        /// The table contains a format identifier (CaretValueFormat) and a contour point index (CaretValuePoint).
        /// </summary>
        ContourPointIndex = 2,

        /// <summary>Specifies the value in design units, but, in non-variable fonts, it uses a Device table rather than a contour point to adjust the value. <para/>
        /// This format offers the advantage of fine-tuning the Coordinate value for any device resolution. 
        /// (For more information about Device tables, see the chapter, Common Table Formats.) <para/>
        /// In variable fonts, CaretValueFormat3 must be used to reference variation data to adjust caret positions for different variation instances, 
        /// if needed. In this case, CaretValueFormat3 specifies an offset to a VariationIndex table, which is a variant of the Device table used for variations. </summary>
        DesignUnitsPlusDVarTable = 3,
    }

    public class CaretValue
    {
        public CaretValueFormat Format { get; private set; }

        /// <summary>Gets the value of the current <see cref="CaretValue"/>. This is either an X or Y coordinate (design units), or a contour point index depending on <see cref="Format"/>.</summary>
        public int Value { get; private set; }

        public DeviceVariationIndexTable VariableTable { get; private set; }

        public CaretValue(CaretValueFormat format, int val, DeviceVariationIndexTable dvt)
        {
            Format = format;
            Value = val;
            VariableTable = dvt;
        }
    }
}
