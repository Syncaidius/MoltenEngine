using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class GraphicsContextException : Exception
    {
        public GraphicsContextException(string message) : base(message) { }
    }
}
