namespace Molten
{
    /// <summary>
    /// Represents the type of operation performed by a <see cref="ContentHandle"/>.
    /// </summary>
    public enum ContentHandleType
    {
        /// <summary>
        /// The handle is for an asset that is to be, or already has been loaded.
        /// </summary>
        Load = 0,

        /// <summary>
        /// The handle is for an asset to be saved to a location.
        /// </summary>
        Save = 1,

        /// <summary>
        /// Deletes an asset.
        /// </summary>
        Delete = 2,
    }
}
