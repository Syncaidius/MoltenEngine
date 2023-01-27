namespace Molten.Graphics
{
    /// <summary>
    /// Represents the implementation of a shader.
    /// </summary>
    public interface IShader : IShaderElement
    {
        /// <summary>Gets the description of the material.</summary>
        string Description { get; set; }

        /// <summary>Gets the author of the material.</summary>
        string Author { get; set; }

        /// <summary>
        /// Gets the file name/path from which the current <see cref="IShader"/> was originally loaded.
        /// </summary>
        string Filename { get; }

        /// <summary>Gets or sets a material value.</summary>
        /// <param name="key">The value key</param>
        /// <returns></returns>
        IShaderValue this[string key] { get; set; }
    }
}
