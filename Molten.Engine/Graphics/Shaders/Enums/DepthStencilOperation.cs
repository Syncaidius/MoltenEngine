namespace Molten.Graphics;

/// <summary>
/// The stencil operations that can be performed during depth-stencil testing.
/// </summary>
public enum DepthStencilOperation
{
    None = 0x0,

    /// <summary>
    /// Keep the existing stencil data.
    /// </summary>
    Keep = 0x1,

    /// <summary>
    /// Set the stencil data to 0.
    /// </summary>
    Zero = 0x2,

    /// <summary>
    /// Set the stencil data to the reference value set by calling ID3D11DeviceContext::OMSetDepthStencilState.
    /// </summary>
    Replace = 0x3,

    /// <summary>
    /// Increment the stencil value by 1, and clamp the result.
    /// </summary>
    IncrementAndClamp = 0x4,

    /// <summary>
    /// Decrement the stencil value by 1, and clamp the result.
    /// </summary>
    DecrementAndClamp = 0x5,

    /// <summary>
    /// Invert the stencil data.
    /// </summary>
    Invert = 0x6,

    /// <summary>
    /// Increment the stencil value by 1, and wrap the result if necessary.
    /// </summary>
    IncrementAndWrap = 0x7,

    /// <summary>
    /// Decrement the stencil value by 1, and wrap the result if necessary.
    /// </summary>
    DecrementAndWrap = 0x8
}
