using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public delegate void FormSurfaceHandler(IWindowSurface surface);

    public interface IWindowSurface : ISwapChainSurface
    {
        /// <summary>Invoked when the user tries to close the form.</summary>
        event FormSurfaceHandler OnClose;

        /// <summary>Invoked when the user minimizes the form.</summary>
        event FormSurfaceHandler OnMinimize;

        /// <summary>Invoked when the users restores the form.</summary>
        event FormSurfaceHandler OnRestore;

        ///// <summary>Invoked when the form is resized either by the user or by the game/engine.</summary>
        //event FormSurfaceHandler OnResize;

        /// <summary>Shows the surface's form if not already visible.</summary>
        void Show();

        /// <summary>Conceals the surface's form from the user if not already hidden.</summary>
        void Hide();

        /// <summary>Gets or sets the title of the underlying form.</summary>
        string Title { get; set; }

        /// <summary>Gets an <see cref="IntPtr"/> to the handle of the underlying form.</summary>
        IntPtr WindowHandle { get; }

        /// <summary>Gets or sets the mode of the underlying form.</summary>
        WindowMode Mode { get; set; }

        /// <summary>Gets the bounds of the window surface.</summary>
        Rectangle Bounds { get; }
    }
}
