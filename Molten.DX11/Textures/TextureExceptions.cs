using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public TextureCopyException(TextureBase source, TextureBase destination)
        : this(source, destination, "Invalid copy operation.")
        { }

        public TextureCopyException(TextureBase source, TextureBase destination, string message)
            : base(message)
        {
            SourceTexture = source;
            DestTexture = destination;

        }

        /// <summary>
        /// The source texture.
        /// </summary>
        public TextureBase SourceTexture { get; private set; }

        /// <summary>
        /// The destination texture.
        /// </summary>
        public TextureBase DestTexture { get; private set; }
    }

    public class TextureDescriptionException : Exception
    {
        public TextureDescriptionException(TextureBase texture, string msg)
        : base(msg)
        {
            Texture = texture;
        }

        public TextureBase Texture { get; private set; }

    }
}
