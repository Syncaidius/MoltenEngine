using System.Diagnostics;
using Molten.Input;
using Molten.UI;

namespace Molten
{
    public class UIPointerTracker
    {
        float _dragThreshold = 10; // Pixels
        Vector2F _dragDistance;
        Vector2F _delta;
        Vector2I _iDelta;
        Vector2F _curPos;
        Vector2F _dragDefecit;

        UIManagerComponent _manager;
        UIElement _pressed = null;
        UIElement _held = null;
        UIElement _hovered = null;
        UIElement _dragging = null;


        /// <summary>
        /// The button set ID, or finger ID.
        /// </summary>
        public int SetID { get; }

        /// <summary>
        /// Gets the button being tracked by the current <see cref="UIPointerTracker"/>.
        /// </summary>
        public PointerButton Button { get; }

        /// <summary>
        /// Gets the pointing device that the current <see cref="UIPointerTracker"/> is tracking.
        /// </summary>
        public PointingDevice Device { get; }

        /// <summary>
        /// The current position of the tracked pointer.
        /// </summary>
        public Vector2F Position => _curPos;

        /// <summary>
        /// The distance moved during the current frame update.
        /// </summary>
        public Vector2F Delta => _delta;

        /// <summary>
        /// An integer version of <see cref="Delta"/>, rounded down.
        /// </summary>
        public Vector2I IntegerDelta => _iDelta;

        /// <summary>
        /// The total distance moved from the initial 'press' location.
        /// </summary>
        public Vector2F DeltaSincePress => _delta;

        /// <summary>
        /// Gets the <see cref="Timing"/> used during the last <see cref="Update(Timing)"/> call.
        /// </summary>
        public Timing Time { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="pDevice"></param>
        /// <param name="setID"></param>
        /// <param name="button"></param>
        internal UIPointerTracker(UIManagerComponent manager, PointingDevice pDevice, int setID, PointerButton button, ref Rectangle inputConstraintBounds)
        {
            _manager = manager;
            Device = pDevice;
            SetID = setID;
            Button = button;
            _curPos = pDevice.Position;
        }

        internal void Update(Timing time, ref Rectangle constraintBounds)
        {
            Time = time;

            _delta = Device.Position - _curPos;
            _curPos = Device.Position - (Vector2F)constraintBounds.TopLeft;

            _dragDefecit.X = _delta.X + _dragDefecit.X;
            _dragDefecit.Y = _delta.Y + _dragDefecit.Y;

            _iDelta.X = (int)_dragDefecit.X;
            _iDelta.Y = (int)_dragDefecit.Y;

            _dragDefecit.X -= _iDelta.X;
            _dragDefecit.Y -= _iDelta.Y;
 
            UIElement prevHover = _hovered;

            _hovered = _manager.Root.Pick(_curPos);

            // Trigger on-leave of previous hover element.
            if (_hovered != prevHover)
                prevHover?.OnLeave(this);

            // Update currently-hovered element
            if (_hovered != null)
            {
                if (prevHover != _hovered)
                    _hovered.OnEnter(this);

                _hovered.OnHover(this);
            }

            if (Device is MouseDevice mouse)
            {
                // Handle scroll wheel event
                if (mouse.ScrollWheel.Delta != 0)
                {
                    // TODO pass mouse.ScrollWheel values to UIElement.OnScroll;
                }
            }

            switch (Button)
            {
                case PointerButton.Left:
                    HandleLeftClick();
                    break;

                case PointerButton.Right:
                    // TODO context menu handling
                    break;

                case PointerButton.Middle:
                    // Focus element but don't handle click actions
                    break;
            }
        }

        private void HandleLeftClick()
        {
            // Handle mouse-specific actions, such as hovering
            // Check if we're starting a new click
            if (Device.IsDown(Button, SetID))
            {
                if (_pressed == null)
                {
                    if (_hovered != null)
                    {
                        _pressed = _hovered;

                        // Check if focused control needs unfocusing.
                        if (_manager.FocusedElement != _pressed)
                            _manager.FocusedElement?.Unfocus();

                        // Trigger press-start event
                        _pressed.Focus();
                        _pressed.OnPressed(this);
                    }

                    _dragDistance = new Vector2F();
                }
                else
                {
                    // Update dragging
                    _dragDistance += _delta;

                    float distDragged = Math.Abs(_dragDistance.Length());
                    if (distDragged >= _dragThreshold)
                    {
                        if (_pressed != null)
                        {
                            if (_dragging == null)
                            {
                                if (_pressed.Contains(Position))
                                {
                                    _dragging = _pressed;

                                    // TODO perform start of drag-drop if element allows being drag-dropped
                                }
                            }

                            _dragging?.OnDragged(this);
                        }
                    }
                }
            }
            else // Handle button release
            {
                Release();
            }
        }

        /// <summary>
        /// Releases tracker state and calls the appropriate <see cref="UIElement"/> callbacks to correctly release state.
        /// </summary>
        internal void Release()
        {
            if (_pressed != null)
            {
                bool inside = _pressed.Contains(_curPos);
                _pressed.OnReleased(this, !inside);

                if (_dragging != null)
                {
                    // TODO perform drop action of drag-drop, if element allows being drag-dropped and target can receive drag-drop actions.
                }
            }

            _pressed = null;
            _dragging = null;
            _held = null;
        }
    }
}

