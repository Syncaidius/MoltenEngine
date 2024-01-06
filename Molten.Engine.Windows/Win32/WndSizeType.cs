namespace Molten.Windows32;

public enum WndSizeType
{
    /// <summary>
    /// The window has been resized, but neither the SIZE_MINIMIZED nor SIZE_MAXIMIZED value applies.
    /// </summary>
    SIZE_RESTORED = 0,

    /// <summary>
    /// The window has been minimized.
    /// </summary>
    SIZE_MINIMIZED = 1,

    /// <summary>
    /// The window has been maximized.
    /// </summary>
    SIZE_MAXIMIZED = 2,

    /// <summary>
    /// Message is sent to all pop-up windows when some other window has been restored to its former size.
    /// </summary>
    SIZE_MAXSHOW = 3,

    /// <summary>
    /// Message is sent to all pop-up windows when some other window is maximized.
    /// </summary>
    SIZE_MAXHIDE = 4,
}
