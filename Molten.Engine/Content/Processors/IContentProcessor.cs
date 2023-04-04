namespace Molten
{
    /// <summary>
    /// Defines a content processor implementation.
    /// </summary>
    public interface IContentProcessor
    {
        /// <summary>
        /// Reads a content part from a stream.
        /// </summary>
        /// <param name="handle">The handle for the content to load.</param>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>True if the content part was successfully read; otherwise, false.</returns>
        bool ReadPart(ContentLoadHandle handle, Stream stream);

        /// <summary>
        /// Builds an asset from the loaded content parts.
        /// </summary>
        /// <param name="assetHandle">The handle of the asset to be built.</param>
        /// <returns>True if the asset was successfully built; otherwise, false.</returns>
        bool BuildAsset(ContentLoadHandle assetHandle);

        /// <summary>
        /// Writes processed content to a stream.
        /// </summary>
        /// <param name="handle">The handle for the content to save.</param>
        /// <param name="stream">The stream to write to.</param>
        /// <returns>True if the content was successfully written; otherwise, false.</returns>
        bool Write(ContentSaveHandle handle, Stream stream);

        /// <summary>
        /// Gets the types of content that this processor can accept.
        /// </summary>
        Type[] AcceptedTypes { get; }

        /// <summary>
        /// Gets the types of services that this processor requires.
        /// </summary>
        Type[] RequiredServices { get; }

        /// <summary>
        /// Gets the type of parameter that this processor requires.
        /// </summary>
        Type ParameterType { get; }

        /// <summary>
        /// Gets the type of content part that this processor handles.
        /// </summary>
        Type PartType { get; }
    }
}
