using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public class GamepadStick : IGamepadStick
    {
        int _maxValue;

        Vector2I _rawValue;
        Vector2F _value;
        Vector2F _deadzone = new Vector2F(0.5f);

        internal GamepadStick(int maxValue)
        {
            _maxValue = maxValue;
        }

        internal void SetValues(int xValue, int yValue)
        {
            _rawValue.X = xValue;
            _rawValue.Y = yValue;

            double dX = _maxValue * _deadzone.X;
            double dY = _maxValue * _deadzone.Y;

            double finalRangeX = _maxValue - dX;
            double finalRangeY = _maxValue - dY;

            if (xValue <= -dX)
                _value.X = (float)(Math.Min(0, xValue + dX) / finalRangeX);
            else
                _value.X = (float)(Math.Max(0, xValue - dX) / finalRangeX);

            if (yValue <= -dY)
                _value.Y = (float)(Math.Min(0, yValue + dY) / finalRangeY);
            else
                _value.Y = (float)(Math.Max(0, yValue - dY) / finalRangeY);
        }

        internal void Clear()
        {
            _rawValue = Vector2I.Zero;
            _value = Vector2F.Zero;
        }

        public Vector2I RawValue => _rawValue;

        public float RawX => _rawValue.X;

        public float RawY => _rawValue.Y;

        public Vector2F Value => _value;

        public float X => _value.X;

        public float Y => _value.Y;

        public Vector2F Deadzone
        {
            get => _deadzone;
            set
            {
                _deadzone = new Vector2F()
                {
                    X = MathHelper.Clamp(value.X, 0, 1.0f),
                    Y = MathHelper.Clamp(value.Y, 0, 1.0f),
                };
            }
        }
    }
}
