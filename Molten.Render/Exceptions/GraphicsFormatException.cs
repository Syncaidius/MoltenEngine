using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class GraphicsFormatException : Exception
    {
        public GraphicsFormatException(GraphicsFormat format) : base($"The provided format ({format}) is incompatible.") { }

        public GraphicsFormatException(IList<GraphicsFormat> formats) : base($"The provided formats ({string.Join(", ", formats)}) is incompatible.") { }
    }
}
