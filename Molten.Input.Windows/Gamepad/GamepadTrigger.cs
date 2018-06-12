using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public class GamepadTrigger : IGamepadTrigger
    {
        float _value;
        float _delta;

        internal void SetPercentage(float newvalue)
        {
            _delta = newvalue - _value;
            _value = newvalue;
        }

        internal void Clear()
        {
            _delta = 0;
            _value = 0;
        }

        public float Value => _value;

        public float Delta => _delta;
    }
}
