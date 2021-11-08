using Molten.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public delegate void TouchGestureHandler<T>(ITouchDevice device, T gesture) where T : ITouchGesture;

    public interface ITouchDevice : IInputDevice<int>
    {
        public event TouchGestureHandler<Touch2PointGesture> OnPinchGesture;

        /// <summary>
        /// Triggered when a touch point is updated, regardless of type.
        /// </summary>
        public event MoltenEventHandler<TouchPointSample> OnTouch;

        TouchPointState GetState(int pointID);

        /// <summary>
        /// Gets the maximum number of simultaneous touch-points (or fingers) supported by the current <see cref="ITouchDevice"/>.
        /// </summary>
        int MaxTouchPoints { get; }

        /// <summary>
        /// The number of active touch points on the current <see cref="ITouchDevice"/>.
        /// </summary>
        int TouchPointCount { get; }
    }
}
