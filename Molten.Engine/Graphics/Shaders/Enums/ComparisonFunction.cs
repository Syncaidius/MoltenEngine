namespace Molten.Graphics;

/// <summary>
/// Represents various comparison functions used for depth/stencil testing.
/// </summary>
public enum ComparisonFunction
{
    /// <summary>
    /// The comparison never passes.
    /// </summary>
    Never = 0x1,

    /// <summary>
    /// The comparison passes if the source value is less than the destination value.
    /// </summary>
    Less = 0x2,

    /// <summary>
    /// The comparison passes if the source value is equal to the destination value.
    /// </summary>
    Equal = 0x3,

    /// <summary>
    /// The comparison passes if the source value is less than or equal to the destination value.
    /// </summary>
    LessEqual = 0x4,

    /// <summary>
    /// The comparison passes if the source value is greater than the destination value.
    /// </summary>
    Greater = 0x5,

    /// <summary>
    /// The comparison passes if the source value is not equal to the destination value.
    /// </summary>
    NotEqual = 0x6,

    /// <summary>
    /// The comparison passes if the source value is greater than or equal to the destination value.
    /// </summary>
    GreaterEqual = 0x7,

    /// <summary>
    /// The comparison always passes.
    /// </summary>
    Always = 0x8
}
