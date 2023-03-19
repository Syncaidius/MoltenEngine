using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
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
}
