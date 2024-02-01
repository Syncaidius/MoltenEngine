namespace Molten.Font;
public class KerningPair
{
    /// <summary>
    /// Gets the glyph index (ID) for the left-hand glyph in the kerning pair.
    /// </summary>
    public ushort Left { get; internal set; }

    /// <summary>
    /// Gets the glyph index (ID) for the right-hand glyph in the kerning pair.
    /// </summary>
    public ushort Right { get; internal set; }

    /// <summary>
    /// Gets the kerning value for the above pair, in FUnits. <para/>
    /// If this value is greater than zero, the characters will be moved apart. If this value is less than zero, the character will be moved closer together.
    /// </summary>
    public short Value { get; internal set; }
}
