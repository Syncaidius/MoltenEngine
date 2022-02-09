using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public enum InputAction
    {
        Released = 0,

        Pressed = 1,

        Held = 2,

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

        None = 255,
    }
}
