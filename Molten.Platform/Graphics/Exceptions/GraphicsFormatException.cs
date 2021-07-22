using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class GraphicsFormatException<T> : Exception
        where T: struct
    {
        public GraphicsFormatException(T format, string msg) : base(msg) { }

        public GraphicsFormatException(T format) : base($"The provided format ({format}) is incompatible.") { }

        public GraphicsFormatException(IList<T> formats) : base($"The provided formats ({string.Join(", ", formats)}) is incompatible.") { }

    }
}
