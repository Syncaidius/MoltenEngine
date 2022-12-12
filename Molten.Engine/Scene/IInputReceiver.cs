using Molten.Input;

namespace Molten
{
    /// <summary>
    /// Represents an object which implements custom input handling for a <typeparamref name="T"/>.
    /// </summary>
    public interface IInputReceiver<T>
        where T : InputDevice
    {
        void HandleInput(T device, Timing timing);
    }
}
