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
}
