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
        /// The task or command must be executed next time the object is applied on the GPU.
        /// </summary>
        Apply = 1,

        /// <summary>
        /// The task or command must be executed after the object has been used in a draw or dispatch call.
        /// </summary>
        PostDrawDispatch = 1,
    }
}
