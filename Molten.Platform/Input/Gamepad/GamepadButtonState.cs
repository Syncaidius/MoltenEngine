using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public struct GamepadButtonState
    {
        /// <summary>
        /// Gets the game pad button associated with the current <see cref="GamepadButtonState"/>.
        /// </summary>
        public GamepadButton Button;

        /// <summary>
        /// Gets the pressure of the button, if pressed, between 0.0f and 1.0f. 
        /// This may always be 1.0f if the device button is not pressure-sensitive.
        /// </summary>
        public float Pressure;

        /// <summary>
        /// Gets the current button press state.
        /// </summary>
        public GamepadPressState State;

        /// <summary>
        /// Gets the UTC time at which the button was last pressed.
        /// </summary>
        public DateTime PressTimestamp;

        /// <summary>
        /// Gets the amount of time that the <see cref="GamepadButton"/> has been held.
        /// </summary>
        public TimeSpan HeldTime;
    }
}
