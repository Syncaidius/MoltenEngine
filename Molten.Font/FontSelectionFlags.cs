namespace Molten.Font
{
    [Flags]
    public enum FontSelectionFlags : ushort
    {
        /// <summary>
        /// 
        /// </summary>
        Default = 0,

        /// <summary>
        /// Font contains italic or oblique characters, otherwise they are upright.
        /// </summary>
        Italic = 1,

        /// <summary>
        /// Characters are underscored.
        /// </summary>
        Underscore = 2,

        /// <summary>
        /// Characters have their foreground and background reversed.
        /// </summary>
        Negative = 4,

        /// <summary>
        /// Outline (hollow) characters, otherwise they are solid.
        /// </summary>
        Outlined = 8,

        /// <summary>
        /// Characters are overstruck.
        /// </summary>
        Strikeout = 16,

        /// <summary>
        /// Characters are emboldened.
        /// </summary>
        Bold = 32,

        /// <summary>
        /// Characters are in the standard weight/style for the font.
        /// </summary>
        Regular = 64,

        /// <summary>
        /// If set, it is strongly recommended to use OS/2.sTypoAscender - OS/2.sTypoDescender+ OS/2.sTypoLineGap as a value for default line spacing for this font.
        /// </summary>
        UseTypoMetrics = 128,

        /// <summary>
        /// The font has ‘name’ table strings consistent with a weight/width/slope family without requiring use of ‘name’ IDs 21 and 22. (Please see more detailed description below.)
        /// </summary>
        Wws = 256,

        /// <summary>
        /// Font contains oblique characters.
        /// </summary>
        Oblique = 512,
    }
}
