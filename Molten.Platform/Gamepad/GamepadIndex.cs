using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    /// <summary>
    /// Represents a gamepad index of player one (1) to four (4) on the local system.
    /// </summary>
    public enum GamepadIndex : byte
    {
        /// <summary>
        /// Player one's gamepad index.
        /// </summary>
        One = 0,

        /// <summary>
        /// Player two's gamepad index.
        /// </summary>
        Two = 1,

        /// <summary>
        /// Player threes's gamepad index.
        /// </summary>
        Three = 2,

        /// <summary>
        /// Player four's gamepad index.
        /// </summary>
        Four = 3,
    }
}
