using Android.Content.PM;
using Android.Views;
using Molten.Graphics;

namespace Molten.Input
{
    public class AndroidTouchDevice : TouchDevice
    {
        public override string DeviceName => "Touch Screen";

        public override int TouchPointCount { get; protected set; }

        AndroidViewSurface _boundSurface;
        View _boundView;

        internal AndroidTouchDevice(AndroidInputService manager) : base(manager)
        {

        }

        protected override int GetMaxSimultaneousStates()
        {
            return 5;
        }

        protected override List<InputDeviceFeature> Initialize()
        {
            IsConnected = false;
            Manager.Settings.Input.TouchBufferSize.OnChanged += TouchSampleBufferSize_OnChanged;

            return null;
        }

        private void TouchSampleBufferSize_OnChanged(int oldValue, int newValue)
        {
            BufferSize = newValue;
        }

        protected override void OnBind(INativeSurface surface)
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
                        MaxSimultaneousStates = 5;
                    else if (pm.HasSystemFeature(PackageManager.FeatureTouchscreenMultitouchDistinct))
                        MaxSimultaneousStates = 2;
                    else
                        MaxSimultaneousStates = 1;
                }
            }

            _boundSurface = null;
        }

        protected override void OnUnbind(INativeSurface surface)
        {
            if (_boundSurface != null && _boundSurface == surface)
            {
                if (_boundView != null && _boundView == _boundSurface.TargetView)
                    _boundView.Touch -= Surface_Touch;

                _boundSurface.TargetActivity.OnTargetViewChanged -= TargetActivity_OnTargetViewChanged;
                ClearState();
            }
        }

        protected override void OnDispose()
        {
            OnUnbind(_boundSurface);
        }

        private void TargetActivity_OnTargetViewChanged(View o)
        {
            if (_boundView != null)
                _boundView.Touch -= Surface_Touch;

            o.Touch += Surface_Touch;
        }

        protected override void OnClearState() { }

        public override void OpenControlPanel()
        {
            // TODO Is this possible?
        }

        private void Surface_Touch(object sender, View.TouchEventArgs e)
        {
            // TODO process touch queue and trigger events accordingly.
            // TODO figure out gestures based on number of pressed touch points.
            // TODO individually track each active touch-point so we can form gestures easily.

            TouchPointState tps = new TouchPointState();
            tps.ID = e.Event.ActionIndex;

            switch (e.Event.ActionMasked)
            {
                case MotionEventActions.PointerDown:
                case MotionEventActions.Down: 
                    tps.State = InputAction.Pressed; break;

                case MotionEventActions.PointerUp:
                case MotionEventActions.Up:
                    tps.State = InputAction.Released;
                    break;

                case MotionEventActions.Move:
                    tps.State = InputAction.Moved;
                    break;

                // NOTE: A movement has happened outside of the normal bounds of the UI element.
                case MotionEventActions.Outside:
                    tps.State = InputAction.Moved;
                    break;

                case MotionEventActions.Scroll:
                    tps.State = InputAction.Moved; 
                    break;
            }

            float pX = e.Event.GetX();
            float pY = e.Event.GetY();
            tps.Position = new Vector2F(pX, pY);
            tps.Pressure = e.Event.GetPressure(tps.ID);
            tps.Orientation = e.Event.GetOrientation(tps.ID);
            tps.Size = e.Event.GetSize(tps.ID);

            // We've handled the touch event
            QueueState(tps);
            e.Handled = true;
        }

        protected override void OnUpdate(Timing time) { }
    }
}
