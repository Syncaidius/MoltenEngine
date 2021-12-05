using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    [Flags]
    public enum PipeBindTypeFlags
    {
        None = 0,

        Input = 1,

        Output = 2,
    }
}
