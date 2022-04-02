using Molten.Input;

namespace Molten
{
    /// <summary>
    /// Represents an object which implements custom input handling.
    /// </summary>
    public interface IInputHandler
    {
        /// <summary>
        /// Invoked when the object is to do custom input handling during scene update.
        /// </summary>
        /// <param name="mouse">The primary <see cref="MouseDevice"/>, if available.</param>
        /// <param name="touch">The primary <see cref="TouchDevice"/>, if available.</param>
        /// <param name="keyboard">The primary <see cref="KeyboardDevice"/>, if available.</param>
        /// <param name="gamepad">The primary <see cref="GamepadDevice"/>, if available.</param>
        /// <param name="timing">A <see cref="Timing"/> instance to provide timing information.</param>
        void HandleInput(MouseDevice mouse, TouchDevice touch, KeyboardDevice keyboard, GamepadDevice gamepad, Timing timing);
    }
}
