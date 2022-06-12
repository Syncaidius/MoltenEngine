using Molten.Input;

namespace Molten
{
    public class ScenePointerTracker
    {
        float _dragThreshold = 10; // Pixels
        IPointerReceiver _pressedObj = null;
        Vector2F _dragDistance;
        Vector2F _delta;
        Vector2F _curPos;
        bool _inputDragged = false;


        /// <summary>
        /// The button set ID, or finger ID.
        /// </summary>
        public int SetID { get; }

        /// <summary>
        /// Gets the button being tracked by the current <see cref="ScenePointerTracker"/>.
        /// </summary>
        public PointerButton Button { get; }

        /// <summary>
        /// Gets the pointing device that the current <see cref="ScenePointerTracker"/> is tracking.
        /// </summary>
        public PointingDevice Device { get; }

        /// <summary>
        /// Gets whether or not the current <see cref="ScenePointerTracker"/> has been disabled.
        /// </summary>
        public bool IsDisabled { get; private set; }

        /// <summary>
        /// The current position of the tracked pointer.
        /// </summary>
        public Vector2F Position => _curPos;

        /// <summary>
        /// The distance moved during the current frame update.
        /// </summary>
        public Vector2F Delta => _delta;

        /// <summary>
        /// The total distance moved from the initial 'press' location.
        /// </summary>
        public Vector2F DeltaSincePress => _delta;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="setID">The button set ID, or finger ID.</param>
        /// <param name="button">The button to track.</param>
        internal ScenePointerTracker(PointingDevice pDevice, int setID, PointerButton button)
        {
            Device = pDevice;
            SetID = setID;
            Button = button;
        }

        internal void Update(SceneManager manager, Timing time)
        {
            _curPos = Device.Position;
            _delta = Device.Delta;

            // Handle clicking and dragging.
            if (Device.IsDown(Button, SetID))
            {
                // Check if we're starting a new click 
                if (_pressedObj == null)
                {
                    // Store the component as being dragged
                    _pressedObj = manager.Hovered;

                    if (_pressedObj != null)
                    {
                        // Check if focused control needs unfocusing.
                        if (manager.Focused != _pressedObj && manager.Focused != null)
                        {
                            if (manager.Focused.Contains(_curPos) == false)
                                manager.Unfocus();
                        }

                        // Trigger press-start event
                        _pressedObj.PointerPressed(this, _curPos);
                    }

                    _inputDragged = false;
                    _dragDistance = new Vector2F();
                }
                else
                {
                    // Update drag checks
                    _dragDistance += _delta;

                    float distDragged = Math.Abs(_dragDistance.Length());
                    if (distDragged >= _dragThreshold)
                    {
                        _inputDragged = true;
                        _pressedObj.PointerDrag(this, _curPos, _delta);
                    }
                }
            }
            else
            {
                // Check if the tap was released outside or inside of the component
                if (_pressedObj != null)
                {
                    if (_pressedObj.Contains(_curPos) == true)
                        _pressedObj.PointerReleased(this, _curPos, _inputDragged);
                    else
                        _pressedObj.PointerReleasedOutside(this, _curPos);

                    _pressedObj = null;
                }

                _inputDragged = false;
            }
        }

        /// <summary>
        /// Clears tracker state and calls the appropriate <see cref="IPointerReceiver"/> callbacks to correctly release state.
        /// </summary>
        internal void Clear()
        {
            IsDisabled = true;

            if (_pressedObj != null)
            {
                _pressedObj.PointerReleased(this, _curPos, false);
                _pressedObj = null;
            }
        }
    }
}

