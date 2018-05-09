using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    internal class ContentTypeGroup
    {
        internal Type GroupType;

        internal Dictionary<string, ContentSegment> Files;

        internal ContentTypeGroup(Type groupType)
        {
            Files = new Dictionary<string, ContentSegment>();
            GroupType = groupType;
        }
    }
}
