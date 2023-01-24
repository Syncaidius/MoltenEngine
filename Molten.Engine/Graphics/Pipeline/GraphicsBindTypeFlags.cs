namespace Molten.Graphics
{
    [Flags]
    public enum GraphicsBindTypeFlags
    {
        /// <summary>
        /// No bind type.
        /// </summary>
        None = 0,

        /// <summary>
        /// An input binding.
        /// </summary>
        Input = 1,

        /// <summary>
        /// An output binding.
        /// </summary>
        Output = 2,
    }
}
