using Android.Content.PM;
using Android.Views;
using Molten.Graphics;
using Molten.Utility;
using System;

namespace Molten.Input
{
    public class AndroidTouchDevice : AndroidInputDeviceBase<int>, ITouchDevice
    {
        public override bool IsConnected { get; protected set; }

        public override string DeviceName => throw new NotImplementedException();

        public int MaxTouchPoints { get; private set; }

        public int TouchPointCount { get; private set; }

        public int BufferSize { get; private set; }

        public int MaxBufferSize
        {
            get => _buffer.Length;
            private set
            {
                if (_buffer.Length != value)
                {
                    ClearState();
                    Array.Resize(ref _buffer, value);
                }
            }
        }

        public event TouchGestureHandler<Touch2PointGesture> OnPinchGesture;

        /// <summary>
        /// Triggered when any touch event occurrs on the current <see cref="AndroidTouchDevice"/>.
        /// </summary>
        public event MoltenEventHandler<TouchPointState> OnTouch;

        /// <summary>
        /// Triggered when an active touch point is moved on the current <see cref="AndroidTouchDevice"/>.
        /// </summary>
        public event MoltenEventHandler<TouchPointState> OnMove;

        /// <summary>
        /// Triggered when a new touch point is pressed down on the current <see cref="AndroidTouchDevice"/>.
        /// </summary>
        public event MoltenEventHandler<TouchPointState> TouchDown;

        /// <summary>
        /// Triggered when an active touch point is released on the current <see cref="AndroidTouchDevice"/>.
        /// </summary>
        public event MoltenEventHandler<TouchPointState> TouchUp;

        /// <summary>
        /// Triggered when an active touch point is held for a period of time on the current <see cref="AndroidTouchDevice"/>.
        /// </summary>
        public event MoltenEventHandler<TouchPointState> TouchHeld;

        AndroidViewSurface _boundSurface;
        View _boundView;
        TouchPointState[] _buffer;
        TouchPointState[] _states;
        int _bStart;
        int _bEnd;

        internal override void Initialize(AndroidInputManager manager, Logger log)
        {
            base.Initialize(manager, log);
            _buffer = new TouchPointState[manager.Settings.TouchBufferSize];
            _states = new TouchPointState[5];

            ClearState();

            IsConnected = false;
            manager.Settings.TouchBufferSize.OnChanged += TouchSampleBufferSize_OnChanged;
        }

        private void TouchSampleBufferSize_OnChanged(int oldValue, int newValue)
        {
            MaxBufferSize = newValue;
        }

        internal override void Bind(INativeSurface surface)
        {
            if (_boundSurface != surface)
            {
                if (surface is AndroidViewSurface vSurface)
                {
                    _boundView = vSurface.TargetView;
                    _boundView.Touch += Surface_Touch;
                    vSurface.TargetActivity.OnTargetViewChanged += TargetActivity_OnTargetViewChanged;
                    _boundSurface = vSurface;

                    PackageManager pm = vSurface.TargetActivity.UnderlyingActivity.PackageManager;
                    bool wasConnected = IsConnected;
                    IsConnected = pm.HasSystemFeature(PackageManager.FeatureTouchscreen);

                    if (wasConnected && IsConnected == false)
                        ClearState();

                    if (pm.HasSystemFeature(PackageManager.FeatureTouchscreenMultitouchJazzhand))
                        MaxTouchPoints = 5;
                    else if (pm.HasSystemFeature(PackageManager.FeatureTouchscreenMultitouchDistinct))
                        MaxTouchPoints = 2;
                    else
                        MaxTouchPoints = 1;

                    // Ensure _states is large enough
                    if (_states.Length < MaxTouchPoints)
                        Array.Resize(ref _states, MaxTouchPoints);
                }
            }

            _boundSurface = null;
        }

        internal override void Unbind(INativeSurface surface)
        {
            if (_boundSurface != null && _boundSurface == surface)
            {
                if (_boundView != null && _boundView == _boundSurface.TargetView)
                    _boundView.Touch -= Surface_Touch;

                _boundSurface.TargetActivity.OnTargetViewChanged -= TargetActivity_OnTargetViewChanged;
                ClearState();
            }
        }

        private void TargetActivity_OnTargetViewChanged(View o)
        {
            if (_boundView != null)
                _boundView.Touch -= Surface_Touch;

            o.Touch += Surface_Touch;
        }

        public override void ClearState()
        {
            _bStart = 0;
            _bEnd = 0;

            for (int i = 0; i < _states.Length; i++)
            {
                _states[i] = new TouchPointState()
                {
                    Delta = Vector2F.Zero,
                    ID = i,
                    Position = Vector2F.Zero,
                    State = TouchState.Released,
                };
            }
        }

        public override void OpenControlPanel()
        {
            // TODO Is this possible?
        }

        public TouchPointState GetState(int pointID)
        {
            if (pointID > MaxTouchPoints)
                throw new IndexOutOfRangeException("pointID was greater than or equal to MaxTouchPoints.");

            return _states[pointID];
        }

        private void Surface_Touch(object sender, View.TouchEventArgs e)
        {
            // Should we circle back to the beginning of the buffer?
            if (_bEnd == _buffer.Length)
                _bEnd = 0;

            TouchPointState tps = new TouchPointState();
            tps.ID = e.Event.ActionIndex;

            switch (e.Event.ActionMasked)
            {
                case MotionEventActions.PointerDown:
                case MotionEventActions.Down: 
                    tps.State = TouchState.Pressed; break;

                case MotionEventActions.PointerUp:
                case MotionEventActions.Up:
                    tps.State = TouchState.Released;
                    break;

                case MotionEventActions.Move:
                    tps.State = TouchState.Moved;
                    break;

                // NOTE: A movement has happened outside of the normal bounds of the UI element.
                case MotionEventActions.Outside:
                    tps.State = TouchState.Moved;
                    break;

                case MotionEventActions.Scroll:
                    tps.State = TouchState.Moved; 
                    break;
            }

            float pX = e.Event.GetX();
            float pY = e.Event.GetY();
            tps.Position = new Vector2F(pX, pY);
            tps.Pressure = e.Event.GetPressure(tps.ID);
            tps.Orientation = e.Event.GetOrientation(tps.ID);
            tps.Size = e.Event.GetSize(tps.ID);

            // We've handled the touch event
            _buffer[_bEnd++] = tps;
            e.Handled = true;
        }

        internal override void Update(Timing time)
        {
            // TODO process touch queue and trigger events accordingly.
            // TODO figure out gestures based on number of pressed touch points.
            // TODO individually track each active touch-point so we can form gestures easily.
            while (_bStart != _bEnd)
            {
                if (_bStart == _buffer.Length)
                    _bStart = 0;

                TouchPointState tps = _buffer[_bStart];
                TouchPointState last = _states[tps.ID];

                // Calculate delta from last pointer state.
                if (tps.State == TouchState.Moved && last.State != TouchState.Released)
                {
                    tps.Delta = tps.Position - _states[tps.ID].Position;
                    OnTouch?.Invoke(tps);
                    OnMove?.Invoke(tps);
                }
                else
                {
                    tps.Delta = Vector2F.Zero;
                    OnTouch?.Invoke(tps);

                    switch (tps.State)
                    {
                        case TouchState.Pressed: TouchDown?.Invoke(tps); break;
                        case TouchState.Released: TouchUp?.Invoke(tps); break;
                        case TouchState.Held: TouchHeld?.Invoke(tps); break;
                    }
                }

                // Update latest-known state for current pointer ID.
                _states[tps.ID] = tps;
                _bStart++;
            }
        }

        public override bool IsAnyDown(params int[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                int id = values[i];

                if (id > _states.Length)
                    continue;

                if (_states[id].State == TouchState.Pressed)
                    return true;
            }

            return false;
        }

        public override bool IsHeld(int value)
        {
            return _states[value].State == TouchState.Held || _states[value].State == TouchState.Moved;
        }

        public override bool IsDown(int value)
        {
            return _states[value].State == TouchState.Pressed;
        }

        public override bool IsTapped(int value)
        {
            return _states[value].State == TouchState.Pressed;
        }
    }
}
