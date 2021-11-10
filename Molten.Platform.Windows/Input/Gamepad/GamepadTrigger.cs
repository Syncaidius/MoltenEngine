using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public class GamepadTrigger : IGamepadTrigger
    {
        int _maxValue;
        float _rawValue;
        float _value;
        float _deadzone;

        internal GamepadTrigger(int maxValue)
        {
            _maxValue = maxValue;
            _deadzone = 0;
        }

        internal void SetValue(float newValue)
        {
            _rawValue = MathHelper.Clamp(newValue, -_maxValue, _maxValue);

            double dead = _maxValue * _deadzone;
            double finalRange = _maxValue - dead;

            if (_rawValue <= -dead)
                _value = (float)(Math.Min(0, _rawValue + dead) / finalRange);
            else
                _value = (float)(Math.Max(0, _rawValue - dead) / finalRange);
        }

        internal void Clear()
        {
            _value = 0;
            _rawValue = 0;
        }

        public float Value => _value;

        public float RawValue => _rawValue;

        public float Deadzone
        {
            get => _deadzone;
            set => _deadzone = value;
        }
    }
}
