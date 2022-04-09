using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public enum FillRule
    {
        FILL_NONZERO,
        FILL_ODD, // "even-odd"
        FILL_POSITIVE,
        FILL_NEGATIVE
    };
}
