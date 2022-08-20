using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
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

        /// <summary>
        /// The element is opening. E.g. a window, collapsible pane or drop-down element.
        /// </summary>
        Opening = 5,

        /// <summary>
        /// The element is closing. E.g. a window, collapsible pane or drop-down element.
        /// </summary>
        Closing = 6,
    }
}
