namespace Molten.UI;

/// <summary>
/// Represents <see cref="UIElement"/> state.
/// </summary>
public enum UIElementState
{
    /// <summary>
    /// The default element state.
    /// </summary>
    Default = 0,

    /// <summary>
    /// The element is pressed.
    /// </summary>
    Pressed = 1,

    /// <summary>
    /// The element is being hovered over by a pointer or cursor.
    /// </summary>
    Hovered = 2,

    /// <summary>
    /// The element is disabled.
    /// </summary>
    Disabled = 3,

    /// <summary>
    /// Active, checked or selected.
    /// </summary>
    Active = 4,
}
