using System;

namespace Molten.Input
{
    public struct TouchPointState
    {
        public Vector2F Position;

        public Vector2F Delta;

        public TouchState State;

        public int ID;
    }
}
