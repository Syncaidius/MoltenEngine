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

        void CursorClickStarted(Vector2F pos, MouseButton button);

        void CursorClickCompletedOutside(Vector2F pos, MouseButton button);

        void CursorClickCompleted(Vector2F pos, bool wasDragged, MouseButton button);

        void CursorDrag(Vector2F pos, Vector2F delta, MouseButton button);

        void CursorWheelScroll(InputScrollWheel wheel);

        void CursorEnter(Vector2F pos);

        void CursorLeave(Vector2F pos);

        void CursorHover(Vector2F pos);

        void CursorFocus();

        void CursorUnfocus();

        /// <summary>
        /// Gets the tooltip that is displayed when the object is hovered over by a mouse cursor.
        /// </summary>
        string Tooltip { get; }
    }
}
