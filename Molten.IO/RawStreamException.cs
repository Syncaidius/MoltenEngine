using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.IO
{
    public class RawStreamException : Exception
    {
        internal RawStreamException(RawStream stream, string message) : base($"{message} - Mode: {mapType}")
        {
            Stream = stream;
        }

        public RawStream Stream { get; }
    }
}
