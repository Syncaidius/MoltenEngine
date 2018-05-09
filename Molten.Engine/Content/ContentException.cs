using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class ContentException : Exception
    {
        internal ContentException(string message) : base(message) { }
    }
}
