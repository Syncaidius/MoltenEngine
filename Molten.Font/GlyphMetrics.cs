using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class GlyphMetrics
    {
        public int AdvanceWidth { get; internal set; }

        public int LeftSideBearing { get; internal set; }

        public int RightSideBearing { get; internal set; }

        public int TopSideBearing { get; internal set; }

        public int BottomSideBearing { get; internal set; }
    }
}
