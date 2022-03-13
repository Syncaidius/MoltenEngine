using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public struct KeyboardKeyState : IInputState
    {
        public KeyCode Key;

        public KeyboardKeyType KeyType;

        DateTime _pressTimestamp;
        public DateTime PressTimestamp
        {
            get => _pressTimestamp;
            set => _pressTimestamp = value;
        }

        public InputAction Action;

        ulong _updateID;
        public ulong UpdateID
        {
            get => _updateID;
            set => _updateID = value;
        }

        /// <summary>Gets the character value of the key. 
        /// This is only populated if <see cref="KeyType"/> is equal to <see cref="KeyboardKeyType.Character"/></summary>
        public char Character;
    }
}
