namespace Molten.Input;

public enum InputAction
{
    /// <summary>
    /// A previous press action was released.
    /// </summary>
    Released = 0,

    /// <summary>
    /// A single press.
    /// </summary>
    Pressed = 1,

    /// <summary>
    /// The input is being held.
    /// </summary>
    Held = 2,

    /// <summary>
    /// The input was moved.
    /// </summary>
    Moved = 3,

    /// <summary>
    /// The input performed a vertical scroll action
    /// </summary>
    VerticalScroll = 4,

    /// <summary>
    /// The input performed a horizontal scroll action.
    /// </summary>
    HorizontalScroll = 5,

    /// <summary>
    /// The input performed a hover action.
    /// </summary>
    Hover = 6,

    /// <summary>
    /// No action.
    /// </summary>
    None = 255,
}
