namespace Molten.Graphics
{
    public class ResourceCopyException : Exception
    {
        public ResourceCopyException(GraphicsResource source, GraphicsResource destination)
        : this(source, destination, "Invalid copy operation.")
        { }

        public ResourceCopyException(GraphicsResource source, GraphicsResource destination, string message)
            : base(message)
        {
            SourceTexture = source;
            DestTexture = destination;
        }

        /// <summary>
        /// The source texture.
        /// </summary>
        public GraphicsResource SourceTexture { get; private set; }

        /// <summary>
        /// The destination texture.
        /// </summary>
        public GraphicsResource DestTexture { get; private set; }
    }
}
