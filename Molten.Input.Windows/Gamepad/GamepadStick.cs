using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public class GamepadStick : IGamepadStick
    {
        Vector2F _value;
        Vector2F _delta;
        internal void SetPercentages(float x, float y)
        {
            _delta.X = x - _value.X;
            _delta.Y = y - _value.Y;
            _value.X = x;
            _value.Y = y;
        }

        internal void Clear()
        {
            _delta = Vector2F.Zero;
            _value = Vector2F.Zero;
        }

        public Vector2F Value => _value;

        public float X => _value.X;

        public float Y => _value.Y;

        public Vector2F Delta => _delta;

        public float DeltaX => _delta.X;

        public float DeltaY => _delta.Y;
    }
}
