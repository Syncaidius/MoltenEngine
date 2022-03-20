namespace Molten.Graphics
{
    [Flags]
    public enum StateConditions : byte
    {
        /// <summary>
        /// No conditions.
        /// </summary>
        None = 0,

        /// <summary>
        /// Multisampling is enabled.
        /// </summary>
        Multisampling = 1,

        /// <summary>
        /// Anisotropic filtering is enabled
        /// </summary>
        AnisotropicFiltering = 2,

        /// <summary>
        /// Scissor testing is enabled
        /// </summary>
        ScissorTest = 4,

        /// <summary>
        /// Skybox rendering is enabled.
        /// </summary>
        Skybox = 8,

        /// <summary>
        /// Debug rendering is enabled.
        /// </summary>
        Debug = 16,

        /// <summary>
        /// All conditions are present.
        /// </summary>
        All = Multisampling | AnisotropicFiltering | ScissorTest | Skybox | Debug
    }
}
