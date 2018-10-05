using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    /// <summary>
    /// Represents a thumb-stick a game controller.
    /// </summary>
    public interface IGamepadStick
    {
        /// <summary>
        /// Gets or sets the stick deadzone as a percentage of the stick's total value range along each axis. 
        /// The lower this value is set, the more sensitive the stick should be. 
        /// However, setting it too low may cause the stick to become sensitive to unwanted movement. <para/>
        /// </summary>
        Vector2F Deadzone { get; set; }

        /// <summary>
        /// Gets a <see cref="Vector2F"/> containing the raw value for each stick axis.
        /// </summary>
        Vector2I RawValue { get; }

        /// <summary>
        /// Gets the raw value for the stick's X axis.
        /// </summary>
        float RawX { get; }

        /// <summary>
        /// Gets the raw value for the stick's Y axis.
        /// </summary>
        float RawY { get; }

        /// <summary>
        /// Gets a <see cref="Vector2F"/> containing the percentage value for each stick axis. The deadzone is taken into account when calculating this value.
        /// </summary>
        Vector2F Value { get; }

        /// <summary>
        /// Gets the percentage value for the stick's X axis.
        /// </summary>
        float X { get; }

        /// <summary>
        /// Gets the percentage value for the stick's Y axis.
        /// </summary>
        float Y { get; }
    }
}
