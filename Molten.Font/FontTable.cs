using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public abstract class FontTable { }

    public abstract class FontSubTable : FontTable
    {
        internal abstract void Read(EnhancedBinaryReader reader, FontReaderContext context, FontTable parent);
    }
}
