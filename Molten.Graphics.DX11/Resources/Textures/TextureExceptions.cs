namespace Molten.Graphics
{
    /// <summary>Thrown when invalid texture flags were passed during the creation of a new texture.</summary>
    public class TextureFlagException : Exception
    {
        public TextureFlagException(TextureFlags flags)
            : base("Invalid texture flags")
        {
            Flags = flags;
        }

        public TextureFlagException(TextureFlags flags, string message)
            : base(message)
        {
            Flags = flags;
        }

        /// <summary>Gets the texture flags value which caused the exception.</summary>
        public TextureFlags Flags { get; private set; }
    }

    public class TextureCopyException : Exception
    {
        public TextureCopyException(TextureDX11 source, TextureDX11 destination)
        : this(source, destination, "Invalid copy operation.")
        { }

        public TextureCopyException(TextureDX11 source, TextureDX11 destination, string message)
            : base(message)
        {
            SourceTexture = source;
            DestTexture = destination;

        }

        /// <summary>
        /// The source texture.
        /// </summary>
        public TextureDX11 SourceTexture { get; private set; }

        /// <summary>
        /// The destination texture.
        /// </summary>
        public TextureDX11 DestTexture { get; private set; }
    }

    public class TextureDescriptionException : Exception
    {
        public TextureDescriptionException(TextureDX11 texture, string msg)
        : base(msg)
        {
            Texture = texture;
        }

        public TextureDX11 Texture { get; private set; }

    }
}
