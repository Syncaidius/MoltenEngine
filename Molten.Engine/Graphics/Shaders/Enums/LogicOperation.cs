namespace Molten.Graphics
{
    /// <summary>
    /// Specifies the logical operations that can be performed on a bit or set of bits.
    /// </summary>
    public enum LogicOperation
    {
        /// <summary>
        /// Sets all pixel to 0.
        /// </summary>
        Clear = 0,

        /// <summary>
        /// Sets all pixel to 1.
        /// </summary>
        Set = 1,

        /// <summary>
        /// Copies the source pixel to the destination pixel.
        /// </summary>
        Copy = 2,

        /// <summary>
        /// Copies the inverted source pixel to the destination pixel.
        /// </summary>
        CopyInverted = 3,

        /// <summary>
        /// Does not modify any pixel.
        /// </summary>
        Noop = 4,

        /// <summary>
        /// Inverts all pixel in the destination.
        /// </summary>
        Invert = 5,

        /// <summary>
        /// Performs a logical AND operation between source and destination pixel.
        /// </summary>
        And = 6,

        /// <summary>
        /// Performs a logical NAND operation between source and destination pixel.
        /// </summary>
        Nand = 7,

        /// <summary>
        /// Performs a logical OR operation between source and destination pixel.
        /// </summary>
        Or = 8,

        /// <summary>
        /// Performs a logical NOR operation between source and destination pixel.
        /// </summary>
        Nor = 9,

        /// <summary>
        /// Performs a logical XOR operation between source and destination pixel.
        /// </summary>
        Xor = 10,

        /// <summary>
        /// Performs a logical XNOR operation between source and destination pixel.
        /// </summary>
        Equivalent = 11,

        /// <summary>
        /// Performs a logical AND operation between inverted source and destination pixel.
        /// </summary>
        AndReverse = 12,

        /// <summary>
        /// Performs a logical AND operation between inverted source and inverted destination pixel.
        /// </summary>
        AndInverted = 13,

        /// <summary>
        /// Performs a logical OR operation between inverted source and destination pixel.
        /// </summary>
        OrReverse = 14,

        /// <summary>
        /// Performs a logical OR operation between inverted source and inverted destination pixel.
        /// </summary>
        OrInverted = 0xF
    }
}
