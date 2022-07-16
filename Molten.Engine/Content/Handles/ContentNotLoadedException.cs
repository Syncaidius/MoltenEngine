using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class ContentNotLoadedException : Exception
    {
        public ContentNotLoadedException(string message) : base(message) { }
    }
}
