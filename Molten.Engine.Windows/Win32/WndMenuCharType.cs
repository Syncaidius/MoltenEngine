using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Windows32
{
    public enum WndMenuCharType
    {
        /// <summary>
        /// Informs the system that it should discard the character the user pressed and create a short beep on the system speaker.
        /// </summary>
        MNC_IGNORE = 0,

        /// <summary>
        /// Informs the system that it should close the active menu.
        /// </summary>
        MNC_CLOSE = 1,

        /// <summary>
        /// Informs the system that it should choose the item specified in the low-order word of the return value. The owner window receives a WM_COMMAND message.
        /// </summary>
        MNC_EXECUTE = 2,

        /// <summary>
        /// Informs the system that it should select the item specified in the low-order word of the return value.
        /// </summary>
        MNC_SELECT = 3,
    }
}
