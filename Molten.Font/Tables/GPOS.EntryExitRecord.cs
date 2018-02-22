using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public partial class GPOS
    {
        public class EntryExitRecord
        {
            public AnchorTable EntryAnchor { get; internal set; }

            public AnchorTable ExitAnchor { get; internal set; }
        }
    }
}
