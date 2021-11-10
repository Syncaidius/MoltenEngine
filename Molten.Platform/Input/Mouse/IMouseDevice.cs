namespace Molten.Input
{
    public delegate void MouseEventHandler(IMouseDevice mouse);

    /// <summary>
    /// Represents an implementation of a mouse or pointer device.
    /// </summary>
    public interface IMouseDevice : IInputDevice<MouseButton>
    {
        /// <summary>
        /// Occurs when the mouse cursor was inside it's bound surface, but just left it.
        /// </summary>
        event MouseEventHandler OnLeaveSurface;

        /// <summary>
        /// Occurs when the mouse cursor was outside of it's bound surface, but just entered it.
        /// </summary>
        event MouseEventHandler OnEnterSurface;

        /// <summary>Positions the mouse cursor at the center of the currently-bound <see cref="IInputCamera"/>.</summary>
        void CenterInView();

        /// <summary>Returns the amount the mouse cursor has moved a long X and Y since the last frame/update.</summary>
        Vector2I Delta { get; }

        /// <summary>Gets the amount the mouse wheel has been moved since the last frame.</summary>
        float WheelDelta { get; }

        /// <summary>Gets the current scroll wheel position.</summary>
        float WheelPosition { get; }

        /// <summary>
        /// Gets the position of the mouse cursor.
        /// </summary>
        Vector2I Position { get; }

        /// <summary>Gets or sets whether or not the mouse cursor is visible.</summary>
        bool CursorVisible { get; set; }

        /// <summary>Gets or sets whether or not the mouse is contrained to the bounds of the main output.</summary>
        bool IsConstrained { get; set; }
    }
}
