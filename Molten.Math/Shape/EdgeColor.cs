namespace Molten
{
    /// <summary>
    /// Represents the color of a <see cref="Shape.Edge"/>.
    /// </summary>
    [Flags]
    public enum EdgeColor
    {
        /// <summary>Default, no color.</summary>
        Black = 0,

        /// <summary>Red channel.</summary>
        Red = 1,

        /// <summary>Green channel.</summary>
        Green = 1 << 1,

        /// <summary>Blue channel.</summary>
        Blue = 1 << 2,

        /// <summary>
        /// Edge is a combination of red and green channels.
        /// </summary>
        Yellow = Red | Green,

        /// <summary>
        /// Edge is a combination of blue and red channels.
        /// </summary>
        Magenta = Blue | Red,

        /// <summary>Edge is a combination of green and blue channels.</summary>
        Cyan = Green | Blue,

        /// <summary>Edge is a combination of red, green and blue channels.</summary>
        White = Red | Green | Blue
    };
}
