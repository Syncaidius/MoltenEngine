using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Represents the index format. This can either be a 32-bit or 16-bit unsigned value.</summary>
    public enum IndexBufferFormat
    {
        /// <summary>A unsigned 32-bit integer (uint).</summary>
        Unsigned32Bit = 0,

        /// <summary>A unsigned 16-bit integer (short).</summary>
        Unsigned16Bit = 1,

        Signed32Bit = 2,

        Signed16Bit = 3,
    }
}
