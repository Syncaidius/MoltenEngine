namespace Molten.Graphics
{
    [Flags]
    public enum DepthClearFlags
    {
        None = 0,

        /// <summary>
        /// Clear the depth buffer, using fast clear if possible, then place the resource in a compressed state.
        /// </summary>
        Depth = 1,

        /// <summary>Clear the stencil buffer, using fast clear if possible, then place the resource in a compressed state.</summary>
        Stencil = 2
    }
}
