using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public enum TouchPointState
    {
        None = 0,

        Released = 1,

        Pressed = 2,

        Held = 3,

        Dragged = 4,
    }
}
