using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class FeatureVariationsTable
    {
        internal FeatureVariationsTable(BinaryEndianAgnosticReader reader, Logger log, long startPos)
        {
            reader.Position = startPos;
        }
    }
}
