using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public delegate void WindowSurfaceHandler(IWindowSurface surface);

    /// <summary>
    /// Represents a custom implementation of a native window-based render surface.
    /// </summary>
    public interface IWindowSurface : ISwapChainSurface
    {
        /// <summary>
        /// Occurs when the underlying <see cref="Handle"/> has changed. Invoked by the renderer it is bound to.
        /// </summary>
        event WindowSurfaceHandler OnHandleChanged;

        /// <summary>Invoked when the current <see cref="IWindowSurface"/> has began it's closing process. Invoked by the renderer it is bound to.</summary>
        event WindowSurfaceHandler OnClose;

        /// <summary>Invoked when the current <see cref="IWindowSurface"/> is minimized. Invoked by the renderer it is bound to.</summary>
        event WindowSurfaceHandler OnMinimize;

        /// <summary>Invoked when the current <see cref="IWindowSurface"/> is restored. Invoked by the renderer it is bound to.</summary>
        event WindowSurfaceHandler OnRestore;

        /// <summary>Invoked when the current <see cref="IWindowSurface"/> gains focus.</summary>
        event WindowSurfaceHandler OnFocusGained;

        /// <summary>Invoked when the current <see cref="IWindowSurface"/> loses focus.</summary>
        event WindowSurfaceHandler OnFocusLost;

        /// <summary>Gets or sets the title of the underlying form.</summary>
        string Title { get; set; }

        /// <summary>
        /// Gets whether or not the current <see cref="IWindowSurface"/> is focused.
        /// </summary>
        bool IsFocused { get; }

        /// <summary>Gets or sets the mode of the underlying form.</summary>
        WindowMode Mode { get; set; }

        /// <summary>Gets an <see cref="IntPtr"/> to the handle of the underlying form.</summary>
        IntPtr Handle { get; }

        /// <summary>Gets the bounds of the window surface.</summary>
        Rectangle Bounds { get; }

        /// <summary>
        /// Gets or sets whether or not the form is visible.
        /// </summary>
        bool Visible { get; set; }
    }
}
