using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    internal class ContentSegment
    {
        internal List<object> Objects;

        internal string FilePath;

        internal ContentSegment(string filePath)
        {
            Objects = new List<object>();
            FilePath = filePath;
        }
    }
}
