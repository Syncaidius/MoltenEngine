using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class GlyphPoint
    {
        public Double2 Coordinate { get; internal set; }

        public bool IsOnCurve { get; internal set; }
    }
}
