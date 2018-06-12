using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public interface IGamepadDevice : IInputDevice<GamepadButtonFlags>
    {
        /// <summary>Gets or sets the vibration level of the left force-feedback motor.</summary>
        float VibrationLeft { get; set; }

        /// <summary>Gets or sets the vibration level of the right force-feedback motor.</summary>
        float VibrationRight { get; set; }

        /// <summary>
        /// Gets the X and Y axis values of the left thumbstick.
        /// </summary>
        IGamepadStick LeftThumbstick { get; }

        /// <summary>
        /// Gets the X and Y axis values of the right thumbstick.
        /// </summary>
        IGamepadStick RightThumbstick { get; }

        /// <summary>
        /// Gets the gamepad's left trigger.
        /// </summary>
        IGamepadTrigger LeftTrigger { get; }

        /// <summary>
        /// Gets the gamepad's right trigger.
        /// </summary>
        IGamepadTrigger RightTrigger { get; }

        /// <summary>
        /// Gets the sub-type of the game pad/controller.
        /// </summary>
        GamepadSubType SubType { get; }

        /// <summary>Gets the index of the gamepad.</summary>
        GamepadIndex PlayerIndex { get; }
    }
}
