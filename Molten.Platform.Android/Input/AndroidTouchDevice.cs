using Android.Content.PM;
using Android.Views;
using Molten.Graphics;
using Molten.Utility;
using System;

namespace Molten.Input
{
    public class AndroidTouchDevice : AndroidInputDeviceBase<int>, ITouchDevice
    {
        // NOTE SEE: https://github.com/MonoGame/MonoGame/blob/71b25eece3d1b92d6c9f3f32cb51dc054e099133/MonoGame.Framework/Platform/Android/MonoGameAndroidGameView.cs#L142
        //      SEE: https://github.com/MonoGame/MonoGame/blob/71b25eece3d1b92d6c9f3f32cb51dc054e099133/MonoGame.Framework/Platform/Android/AndroidGameActivity.cs
        //      SEE: https://github.com/MonoGame/MonoGame/blob/71b25eece3d1b92d6c9f3f32cb51dc054e099133/MonoGame.Framework/Platform/Android/Input/Touch/AndroidTouchEventManager.cs
        //      SEE: https://github.com/MonoGame/MonoGame/blob/71b25eece3d1b92d6c9f3f32cb51dc054e099133/MonoGame.Framework/Platform/Android/AndroidGameWindow.cs#L62
        //
        //      SEE: https://github.com/MonoGame/MonoGame/blob/a9e5ae6befc40d7c86320ffdcfcd9d9b66f786a8/MonoGame.Framework/Input/Touch/TouchPanelCapabilities.cs
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

        public event MoltenEventHandler<TouchPointState> OnTouch;

        MotionEvent.PointerCoords _coords;
        AndroidViewSurface _boundSurface;
        TouchPointState[] _buffer;
        TouchPointState[] _states;
        int _bStart;
        int _bEnd;

        internal override void Initialize(AndroidInputManager manager, Logger log)
        {
            base.Initialize(manager, log);
            _buffer = new TouchPointState[manager.Settings.TouchBufferSize];
            _states = new TouchPointState[5];
            _coords = new MotionEvent.PointerCoords();

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
                    vSurface.TargetView.Touch += TargetView_Touch;
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
                _boundSurface.TargetView.Touch -= TargetView_Touch;
                ClearState();
            }
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
                    State = TouchState.None,
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

        private void TargetView_Touch(object sender, View.TouchEventArgs e)
        {
            // Should we circle back to the beginning of the buffer?
            if (_bEnd == _buffer.Length)
                _bEnd = 0;

            TouchPointState tps = new TouchPointState();
            tps.ID = e.Event.ActionIndex;

            switch (e.Event.ActionMasked)
            {
                case MotionEventActions.PointerDown: tps.State = TouchState.Pressed; break;

                case MotionEventActions.PointerUp:
                    tps.State = TouchState.Released;
                    break;

                case MotionEventActions.Move:
                    tps.State = TouchState.Dragged;
                    break;

                // NOTE: A movement has happened outside of the normal bounds of the UI element.
                case MotionEventActions.Outside:
                    tps.State = TouchState.Dragged;
                    break;

                case MotionEventActions.Scroll:
                    tps.State = TouchState.Pressed; 
                    break;
            }

            e.Event.GetPointerCoords(tps.ID, _coords);
            tps.Position = new Vector2F(_coords.X, _coords.Y);
            tps.Pressure = _coords.Pressure;
            tps.Orientation = _coords.Orientation;
            tps.Size = _coords.Size;

            if (_states[tps.ID].State != TouchState.None)
                tps.Delta = tps.Position - _states[tps.ID].Position;
            else
                tps.Delta = Vector2F.Zero;

            // We've handled the touch event
            _buffer[_bEnd++] = tps;
            _states[tps.ID] = tps;
            e.Handled = true;
        }

        internal override void Update(Timing time)
        {
            // TODO process touch queue and trigger events accordingly.
            // TODO figure out gestures based on number of pressed touch points.
            // TODO individually track each active touch-point so we can form gestures easily.
            while(_bStart != _bEnd)
            {
                if (_bStart == _buffer.Length)
                    _bStart = 0;

                _bStart++;
            }
        }
    }
}
