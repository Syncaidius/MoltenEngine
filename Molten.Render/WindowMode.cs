using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public enum WindowMode
    {
        /// <summary>Displays the program in a window, with a border and title bar.</summary>
        Windowed = 0,

        /// <summary>Removes the window border and allows the program to fill the whole screen.</summary>
        Borderless = 1,

        /// <summary>Spread across all possible display outputs.</summary>
        Spread = 2,
    }
}
