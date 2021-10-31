using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    interface ITouchDevice : IInputDevice<TouchFinger>
    {
        FingerState GetFingerState(TouchFinger finger);

        int GetPressedFingerCount();
    }
}
