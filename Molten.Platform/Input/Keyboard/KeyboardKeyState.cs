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

        public InputAction State;

        /// <summary>Gets the character value of the key. 
        /// This is only populated if <see cref="KeyType"/> is equal to <see cref="KeyboardKeyType.Character"/></summary>
        public char Character = char.MinValue;
    }
}
