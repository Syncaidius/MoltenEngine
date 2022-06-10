using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public struct PointerState : IInputState
    {
        /// <summary>
        /// An empty <see cref="PointerState"/>.
        /// </summary>
        public static readonly PointerState Empty = new PointerState();

        /// <summary>
        /// Gets the screen position of the touch point.
        /// </summary>
        public Vector2F Position;

        /// <summary>
        /// The amount that the touch point has moved since it's last state update.
        /// </summary>
        public Vector2F Delta;

        /// <summary>
        /// The state of the touch point.
        /// </summary>
        public InputAction Action { get; set; }

        /// <summary>
        /// The set/finger ID.
        /// </summary>
        public int SetID { get; set; }

        /// <summary>
        /// The pointer button.
        /// </summary>
        public PointerButton Button;

        /// <summary>
        /// The orientation of the pointer, relative to the default upright orientation of the device.
        /// </summary>
        public float Orientation;

        /// <summary>
        /// The pressure applied to the pointer. 
        /// If the device does not support pressure-sensing, this value will always be 1.0f.
        /// </summary>
        public float Pressure;

        /// <summary>
        /// A normalized value that describes the approximate size of the pointer 
        /// touch area in relation to the maximum detectable size of the device.
        /// </summary>
        public float Size;

        public InputActionType ActionType;

        public DateTime PressTimestamp { get; set; }

        public ulong UpdateID { get; set; }
    }
}
