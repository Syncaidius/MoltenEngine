namespace Molten
{
    public interface IWindow
    {
        /// <summary>Gets the bounds of the window surface or its inner render area.</summary>
        Rectangle RenderBounds { get; }

        /// <summary>Gets or sets the title of the window.</summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets whether or not the window is visible.
        /// </summary>
        bool IsVisible { get; set; }
    }
}
