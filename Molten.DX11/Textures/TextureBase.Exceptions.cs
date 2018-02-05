using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract partial class TextureBase
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
                Source = source;
                Destination = destination;
            }

            public TextureBase Source { get; private set; }

            public TextureBase Destination { get; private set; }
        }
    }
}
