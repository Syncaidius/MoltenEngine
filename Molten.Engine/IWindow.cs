namespace Molten;

/// <summary>
/// Represents a window implementation.
/// </summary>
public interface IWindow
{
    /// <summary>
    /// Closes or destroys the current <see cref="IWindow"/>.
    /// </summary>
    void Close();

    /// <summary>Gets the bounds of the window surface or its inner render area.</summary>
    Rectangle RenderBounds { get; }

    /// <summary>Gets or sets the title of the window.</summary>
    string Title { get; set; }

    /// <summary>
    /// Gets or sets whether or not the window is visible.
    /// </summary>
    bool IsVisible { get; set; }
}
