using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
