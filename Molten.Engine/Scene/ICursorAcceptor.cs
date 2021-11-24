using Molten.Input;

namespace Molten
{
    /// <summary>
    /// Represents an object which can respond to mouse/touch cursor input.
    /// </summary>
    public interface ICursorAcceptor
    {
        /// <summary>Called when a scene requires the object to handle cursor input.</summary>
        /// <param name="inputPos">The input position.</param>
        /// <returns></returns>
        ICursorAcceptor HandleInput(Vector2F inputPos);

        bool Contains(Vector2F point);

        void InvokeCursorClickStarted(Vector2F pos, MouseButton button);

        void InvokeCursorClickCompletedOutside(Vector2F pos, MouseButton button);

        void InvokeCursorClickCompleted(Vector2F pos, bool wasDragged, MouseButton button);

        void InvokeCursorWheelScroll(InputScrollWheel wheel);

        void InvokeCursorEnter(Vector2F pos);

        void InvokeCursorLeave(Vector2F pos);

        void InvokeCursorHover(Vector2F pos);

        void InvokeCursorFocus();

        void InvokeCursorDrag(Vector2F pos, Vector2F delta, MouseButton button);

        void InvokeCursorUnfocus();

        /// <summary>
        /// Gets the tooltip that is displayed when the object is hovered over by a mouse cursor.
        /// </summary>
        string Tooltip { get; }
    }
}
