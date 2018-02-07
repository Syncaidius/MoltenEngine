using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public enum InputAcceptorResult
    {
        None = 0,

        Pressed = 1,

        Released = 2,

        Held = 3,

        Dragged = 4,
    }
}
