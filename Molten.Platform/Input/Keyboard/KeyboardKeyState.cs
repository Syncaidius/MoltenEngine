using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public struct KeyboardKeyState
    {
        public KeyCode Key;

        public KeyboardKeyType KeyType;

        public DateTime PressTimestamp;

        public InputPressState State;
    }
}
