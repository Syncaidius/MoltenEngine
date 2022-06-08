using Molten.Input;

namespace Molten
{
    /// <summary>
    /// Represents an object which can respond to mouse/touch cursor input.
    /// </summary>
    public interface IPointerReceiver
    {
        bool Contains(Vector2F point);

        void PointerPressed(Vector2F pos, MouseButton button);

        void PointerReleasedOutside(Vector2F pos, MouseButton button);

        void PointerReleased(Vector2F pos, bool wasDragged, MouseButton button);

        void PointerDrag(Vector2F pos, Vector2F delta, MouseButton button);

        void PointerHeld(Vector2F pos, Vector2F delta, MouseButton button);

        void PointerScroll(InputScrollWheel wheel);

        void PointerEnter(Vector2F pos);

        void PointerLeave(Vector2F pos);

        void PointerHover(Vector2F pos);

        void PointerFocus();

        void PointerUnfocus();

        /// <summary>
        /// Gets the tooltip that is displayed when the object is hovered over by a mouse cursor.
        /// </summary>
        string Tooltip { get; }
    }
}
