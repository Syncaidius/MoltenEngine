using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public delegate void WindowSurfaceHandler(INativeSurface surface);

    /// <summary>
    /// Represents a custom implementation of a native GUI control-based render surface.
    /// </summary>
    public interface INativeSurface : ISwapChainSurface
    {
        /// <summary>
        /// Occurs when the underlying <see cref="Handle"/> has changed. Invoked by the renderer it is bound to.
        /// </summary>
        event WindowSurfaceHandler OnHandleChanged;

        /// <summary>Invoked when the current <see cref="INativeSurface"/> has began it's closing process. Invoked by the renderer it is bound to.</summary>
        event WindowSurfaceHandler OnClose;

        /// <summary>Invoked when the current <see cref="INativeSurface"/> is minimized. Invoked by the renderer it is bound to.</summary>
        event WindowSurfaceHandler OnMinimize;

        /// <summary>Invoked when the current <see cref="INativeSurface"/> is restored. Invoked by the renderer it is bound to.</summary>
        event WindowSurfaceHandler OnRestore;

        /// <summary>Invoked when the current <see cref="INativeSurface"/> gains focus.</summary>
        event WindowSurfaceHandler OnFocusGained;

        /// <summary>Invoked when the current <see cref="INativeSurface"/> loses focus.</summary>
        event WindowSurfaceHandler OnFocusLost;

        /// <summary>Gets or sets the title of the underlying native control.</summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets the internal name of the underlying native control.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets whether or not the current <see cref="INativeSurface"/> is focused.
        /// </summary>
        bool IsFocused { get; }

        /// <summary>Gets or sets the window mode of the underlying control.</summary>
        WindowMode Mode { get; set; }

        /// <summary>Gets an <see cref="IntPtr"/> to the handle of the underlying control.</summary>
        IntPtr Handle { get; }

        /// <summary>Gets the bounds of the window surface.</summary>
        Rectangle Bounds { get; }

        /// <summary>
        /// Gets or sets whether or not the underying control is visible.
        /// </summary>
        bool Visible { get; set; }
    }
}
