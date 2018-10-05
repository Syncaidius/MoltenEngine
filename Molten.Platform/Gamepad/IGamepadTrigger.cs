using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{ 
    /// <summary>
    /// Represents a trigger or pressure button on a gamepad.
    /// </summary>
    public interface IGamepadTrigger
    {
        /// <summary>
        /// Gets the raw value of the trigger. This may differ depending on the device being used.
        /// </summary>
        float RawValue { get; }

        /// <summary>
        /// Gets the trigger value as a percentage of the maximum value. 
        /// </summary>
        float Value { get; }

        /// <summary>
        /// Gets the trigger deadzone, as a percentage.
        /// </summary>
        float Deadzone { get; set; }
    }
}
