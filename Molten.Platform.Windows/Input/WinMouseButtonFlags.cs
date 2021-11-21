using System;

namespace Molten
{
    [Flags]
    internal enum WinMouseButtonFlags
    {
        /// <summary>
        /// The left mouse button is depressed.
        /// </summary>
        MK_LBUTTON = 0x0001,

        /// <summary>
        /// The right mouse button is depressed.
        /// </summary>
        MK_RBUTTON = 0x0002,

        /// <summary>
        /// The SHIFT key is depressed.
        /// </summary>
        MK_SHIFT = 0x0004,

        /// <summary>
        /// The CTRL key is depressed.
        /// </summary>
        MK_CONTROL = 0x0008,

        /// <summary>
        /// The middle mouse button is depressed.
        /// </summary>
        MK_MBUTTON = 0x0010,

        /// <summary>
        /// The first X button is down.
        /// </summary>
        MK_XBUTTON1 = 0x0020,

        /// <summary>
        /// The second X button is down.
        /// </summary>
        MK_XBUTTON2 = 0x0040,
    }
}
