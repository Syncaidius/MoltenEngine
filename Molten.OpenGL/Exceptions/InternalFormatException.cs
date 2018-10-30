using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Molten.Graphics.OpenGL
{
    public class InternalFormatException : Exception
    {
        internal InternalFormatException(SizedInternalFormat format, string msg) : base(msg)
        {
            InternalFormat = format;
        }

        public SizedInternalFormat InternalFormat { get; }
    }
}
