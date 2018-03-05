using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class FontInterpreterException : Exception
    {
        internal FontInterpreterException(string message) : base(message) { }

        internal FontInterpreterException() : base() { }
    }
}
