using Android.Content.PM;
using Android.Views;
using Molten.Graphics;
using Molten.Input;
using Molten.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            get => _queue.Length;
            private set
            {
                if (_queue.Length != value)
                {
                    ClearState();
                    Array.Resize(ref _queue, value);
                }
            }
        }

        public event TouchGestureHandler<Touch2PointGesture> OnPinchGesture;

        public event MoltenEventHandler<TouchPointSample> OnTouch;

        AndroidViewSurface _boundSurface;
        TouchPointSample[] _queue;
        TouchPointState[] _states;

        internal override void Initialize(AndroidInputManager manager, Logger log)
        {
            base.Initialize(manager, log);
            _queue = new TouchPointSample[manager.Settings.TouchBufferSize];
            _states = new TouchPointState[5];
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
            throw new NotImplementedException();
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
            // TODO add touch event to queue.
        }

        internal override void Update(Timing time)
        {
            // TODO process touch queue and trigger events accordingly.
            // TODO figure out gestures based on number of pressed touch points.
            // TODO individually track each active touch-point so we can form gestures easily.
        }
    }
}
