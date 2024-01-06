namespace Molten.UI;

public enum UIWindowState
{
    /// <summary>
    /// The <see cref="UIWindow"/> is open.
    /// </summary>
    Open = 0,

    /// <summary>
    /// The <see cref="UIWindow"/> is currently expanding to its <see cref="Maximized"/> size.
    /// </summary>
    Maximizing = 1,

    /// <summary>
    /// The <see cref="UIWindow"/> is maximized;
    /// </summary>
    Maximized = 2,

    /// <summary>
    /// The <see cref="UIWindow"/> is currently minimizing into its <see cref="Minimized"/> size.
    /// </summary>
    Minimizing = 3,

    /// <summary>
    /// The <see cref="UIWindow"/> is minimized;
    /// </summary>
    Minimized = 4,

    /// <summary>
    /// The element is opening. E.g. a window, collapsible pane or drop-down element.
    /// </summary>
    Opening = 5,

    /// <summary>
    /// The element is closing. E.g. a window, collapsible pane or drop-down element.
    /// </summary>
    Closing = 6,

    /// <summary>
    /// The <see cref="UIWindow"/> has closed.
    /// </summary>
    Closed = 7,
}
