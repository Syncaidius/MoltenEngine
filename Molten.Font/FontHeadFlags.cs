namespace Molten.Font;

[Flags]
/// <summary>Font flags. Usually provided by a font's 'head' table.</summary>
public enum FontHeadFlags : ushort
{
    None = 0,

    /// <summary>
    /// Bit 0: Baseline for font at y=0;
    /// </summary>
    Baseline = 1,

    /// <summary>
    /// Bit 1: Left sidebearing point at x=0 (relevant only for TrueType rasterizers) — see the note below regarding variable fonts;
    /// </summary>
    LeftSidebearing = 2,

    /// <summary>
    /// Bit 2: Instructions may depend on point size;
    /// </summary>
    InstructionSizeDependent = 4,

    /// <summary>
    /// Bit 3: Force ppem to integer values for all internal scaler math; may use fractional ppem sizes if this bit is clear;
    /// </summary>
    IntegralPpemValues = 8,

    /// <summary>
    /// Bit 4: Instructions may alter advance width (the advance widths might not scale linearly);
    /// </summary>
    InstructionAdvanceWidth = 16,

    /// <summary>
    /// Bit 5: This bit is not used in OpenType, and should not be set in order to ensure compatible behavior on all platforms. <para/>
    /// If set, it may result in different behavior for vertical layout in some platforms. (See Apple's specification for details regarding behavior in Apple platforms.)
    /// </summary>
    Reserved5 = 32,

    /// <summary>
    /// Bits 6: 6-10 are not used in Opentype and should always be cleared. (See Apple's specification for details regarding legacy used in Apple platforms.)
    /// </summary>
    Reserved6 = 64,

    /// <summary>
    /// Bits 7: 6-10 are  not used in Opentype and should always be cleared. (See Apple's specification for details regarding legacy used in Apple platforms.)
    /// </summary>
    Reserved7 = 128,

    /// <summary>
    /// Bits 8: 6-10 are  not used in Opentype and should always be cleared. (See Apple's specification for details regarding legacy used in Apple platforms.)
    /// </summary>
    Reserved8 = 256,

    /// <summary>
    /// Bits 9: 6-10 are  not used in Opentype and should always be cleared. (See Apple's specification for details regarding legacy used in Apple platforms.)
    /// </summary>
    Reserved9 = 512,

    /// <summary>
    /// Bits 10: 6-10 are  not used in Opentype and should always be cleared. (See Apple's specification for details regarding legacy used in Apple platforms.)
    /// </summary>
    Reserved10 = 1024,

    /// <summary>
    /// Bit 11: Font data is "lossless" as a results of having been subjected to optimizing transformation and/or compression 
    /// (such as e.g. compression mechanisms defined by ISO/IEC 14496-18, MicroType Express, WOFF 2.0 or similar) where the original font functionality and features are retained but the 
    /// binary compatibility between input and output font files is not guaranteed. As a result of the applied transform, the 'DSIG' table may also be invalidated.
    /// </summary>
    Lossless = 2048,

    /// <summary>
    /// Bit 12: Font converted (produce compatible metrics)
    /// </summary>
    Converted = 4096,

    /// <summary>
    /// Bit 13: Font optimized for ClearType™. <para/>
    /// Note, fonts that rely on embedded bitmaps (EBDT) for rendering should not be considered optimized for ClearType, and therefore should keep this bit cleared.
    /// </summary>
    ClearTypeOptimized = 8192,

    /// <summary>
    /// Bit 14: Last Resort font. <para/>
    /// If set, indicates that the glyphs encoded in the cmap subtables are simply generic symbolic representations of code point ranges and don't truly represent support for those code points. 
    /// If unset, indicates that the glyphs encoded in the cmap subtables represent proper support for those code points.
    /// </summary>
    LastResort = 16384,

    /// <summary>Bit 15: Reserved, set to 0</summary>
    Reserved15 = 32768,
}
