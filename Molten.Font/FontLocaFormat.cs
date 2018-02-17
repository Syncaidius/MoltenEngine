using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>The expected format of the index-to-location (loca) table, if present.</summary>
    public enum FontLocaFormat
    {
        UnsignedInt16 = 0,

        UnsignedInt32 = 1,
    }
}
