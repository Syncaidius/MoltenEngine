using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public struct MouseButtonState : IInputState
    {
        public MouseButton Button;

        DateTime _pressTimestamp;
        public DateTime PressTimestamp
        {
            get => _pressTimestamp;
            set => _pressTimestamp = value;
        }

        public Vector2I Position;

        public Vector2I Delta;

        public InputAction Action;

        public InputActionType ActionType;

        ulong _updateID;
        public ulong UpdateID
        {
            get => _updateID;
            set => _updateID = value;
        }
    }
}
