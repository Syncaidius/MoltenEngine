using OpenGL;
using System;

namespace Molten.Graphics
{
    public class InternalFormatException : Exception
    {
        internal InternalFormatException(InternalFormat format, string msg) : base(msg)
        {
            InternalFormat = format;
        }

        public InternalFormat InternalFormat { get; }
    }
}
