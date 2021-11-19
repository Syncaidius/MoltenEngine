using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public enum InputPressState
    {
        Released = 0,

        Pressed = 1,

        Held = 2,

        Moved = 3,

        None = 4,
    }
}
