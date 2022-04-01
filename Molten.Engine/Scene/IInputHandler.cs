using Molten.Input;

namespace Molten
{
    /// <summary>
    /// Represents a scene object/component that can process input.
    /// </summary>
    public interface IInputHandler
    {
        /// <summary>Called when updating scene input. This gives the current <see cref="IPointerReceiver"/> a chance to do custom input handling..</summary>
        /// <param name="inputPos">The input position.</param>
        /// <returns></returns>
        void HandleInput(Vector2F inputPos);
    }
}
