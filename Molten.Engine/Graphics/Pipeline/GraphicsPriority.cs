namespace Molten.Graphics
{
    /// <summary>
    /// Represents the priority of a graphics task or command.
    /// </summary>
    public enum GraphicsPriority
    {
        /// <summary>
        /// The task or command must be executed immediately.
        /// </summary>
        Immediate = 0,

        /// <summary>
        /// The task or command must be executed at the start of the next render cycle.
        /// </summary>
        PreRender = 1,

        /// <summary>
        /// The task or command must be executed at the end of the current render cycle.
        /// </summary>
        PostRender = 1,
    }
}
