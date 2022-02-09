using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public struct MouseButtonState
    {
        public MouseButton Button;

        public DateTime PressTimestamp;

        public Vector2I Position;

        public Vector2I Delta;

        public InputAction Action;

        public InputActionType ActionType;

        public uint UpdateID;
    }
}
