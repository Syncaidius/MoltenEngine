namespace Molten.Graphics
{
    /// <summary>
    /// Represents a supported shader header node
    /// </summary>
    public enum ShaderNodeType
    {
        Name = 0,

        Description = 1,

        Author = 3,

        Pass = 4,

        Version = 5,

        Iterations = 6,

        /// <summary>
        /// Vertex shader entry-point
        /// </summary>
        Vertex = 7,

        /// <summary>
        /// Geometry shader entry-point
        /// </summary>
        Geometry = 8,

        /// <summary>
        /// Hull shader entry-point
        /// </summary>
        Hull = 9,

        /// <summary>
        /// Domain shader entry-point
        /// </summary>
        Domain = 10,

        /// <summary>
        /// Pixel/fragment shader - entry-point
        /// </summary>
        Pixel = 11,

        /// <summary>
        /// Pixel/fragment shader - entry-point
        /// </summary>
        Fragment = Pixel,

        /// <summary>
        /// Blend state definition
        /// </summary>
        State = 12,

        /// <summary>
        /// Texture sampler definition
        /// </summary>
        Sampler = 15,

        /// <summary>
        /// Entry-point definition.
        /// </summary>
        Entry = 16,
    }
}
