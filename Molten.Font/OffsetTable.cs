using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class OffsetTable
    {
        public ushort MajorVersion { get; internal set; }

        public ushort MinorVersion { get; internal set; }

        public ushort NumTables { get; internal set; }

        public ushort SearchRange { get; internal set; }

        public ushort EntrySelector { get; internal set; }

        public ushort RangeShift { get; internal set; }
    }
}
