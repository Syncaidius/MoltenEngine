namespace Molten.Graphics
{
    /// <summary>
    /// Defines an enumeration of blend operations that can be performed.
    /// </summary>
    public enum BlendOperation
    {
        /// <summary>
        /// The blend operation is invalid.
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// The source and destination colors are added.
        /// </summary>
        Add = 0x1,

        /// <summary>
        /// The source color is subtracted from the destination color.
        /// </summary>
        Subtract = 0x2,

        /// <summary>
        /// The destination color is subtracted from the source color.
        /// </summary>
        RevSubtract = 0x3,

        /// <summary>
        /// The minimum of the source and destination colors is used.
        /// </summary>
        Min = 0x4,

        /// <summary>
        /// The maximum of the source and destination colors is used.
        /// </summary>
        Max = 0x5
    }
}
