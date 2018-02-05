using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public enum ContentRequestType
    {
        Read = 0,

        Write = 1,

        Delete = 2,

        Serialize = 3,

        Deserialize = 4,
    }
}
