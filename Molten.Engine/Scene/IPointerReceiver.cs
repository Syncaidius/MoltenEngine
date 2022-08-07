using Molten.Input;

namespace Molten
{
    /// <summary>
    /// Represents an object which can respond to mouse/touch cursor input.
    /// </summary>
    public interface IPointerReceiver
    {
        bool Contains(Vector2F point);

        void PointerPressed(ScenePointerTracker button);

        void PointerReleasedOutside(ScenePointerTracker button);

        void PointerReleased(ScenePointerTracker button, bool wasDragged);

        void PointerDrag(ScenePointerTracker button);

        void PointerHeld(ScenePointerTracker button);

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
