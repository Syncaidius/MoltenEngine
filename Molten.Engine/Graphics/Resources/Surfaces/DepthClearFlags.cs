namespace Molten.Graphics;

[Flags]
public enum DepthClearFlags
{
    /// <summary>
    /// Don't clear any part of the depth-stencil buffer.
    /// </summary>
    None = 0,

    /// <summary>
    /// Clear the depth buffer, using fast clear if possible, then place the resource in a compressed state.
    /// </summary>
    Depth = 1,

    /// <summary>Clear the stencil buffer, using fast clear if possible, then place the resource in a compressed state.</summary>
    Stencil = 2
}

public static class DepthClearFlagsExtensions
{
    public static bool Has(this DepthClearFlags flags, DepthClearFlags value)
    {
        return (flags & value) == value;
    }
}
