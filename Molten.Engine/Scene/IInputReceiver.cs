using Molten.Input;

namespace Molten
{
    /// <summary>
    /// Represents an object which implements custom input handling.
    /// </summary>
    public interface IInputReceiver
    {
        void HandleInput(MouseDevice mouse, TouchDevice touch, KeyboardDevice keyboard, GamepadDevice gamepad, Timing timing);
    }
}
