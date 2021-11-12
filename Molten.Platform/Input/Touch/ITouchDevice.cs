using Molten.Utility;

namespace Molten.Input
{
    public delegate void TouchGestureHandler<T>(ITouchDevice device, T gesture) where T : ITouchGesture;

    public interface ITouchDevice : IInputDevice<int>
    {
        public event TouchGestureHandler<Touch2PointGesture> OnPinchGesture;

        /// <summary>
        /// Triggered when a touch point is updated, regardless of type.
        /// </summary>
        event MoltenEventHandler<TouchPointState> OnTouch;

        /// <summary>
        /// Triggered when an active touch point is moved on the current <see cref="ITouchGesture"/>.
        /// </summary>
        event MoltenEventHandler<TouchPointState> OnMove;

        /// <summary>
        /// Triggered when a new touch point is pressed down on the current <see cref="ITouchGesture"/>.
        /// </summary>
        event MoltenEventHandler<TouchPointState> TouchDown;

        /// <summary>
        /// Triggered when an active touch point is released on the current <see cref="ITouchGesture"/>.
        /// </summary>
        event MoltenEventHandler<TouchPointState> TouchUp;

        /// <summary>
        /// Triggered when an active touch point is held for a period of time on the current <see cref="ITouchGesture"/>.
        /// </summary>
        event MoltenEventHandler<TouchPointState> TouchHeld;

        TouchPointState GetState(int pointID);

        /// <summary>
        /// Gets the maximum number of simultaneous touch-points (or fingers) supported by the current <see cref="ITouchDevice"/>.
        /// </summary>
        int MaxTouchPoints { get; }

        /// <summary>
        /// The number of active touch points on the current <see cref="ITouchDevice"/>.
        /// </summary>
        int TouchPointCount { get; }

        /// <summary>
        /// Gets the maximum size of the touch buffer.
        /// </summary>
        int MaxBufferSize { get; }

        /// <summary>
        /// Gets the current size of the touch buffer.
        /// </summary>
        int BufferSize { get; }
    }
}
