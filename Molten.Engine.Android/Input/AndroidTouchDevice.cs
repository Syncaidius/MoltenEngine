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

        protected override StateParameters GetStateParameters()
        {
            return new StateParameters()
            {
                StatesPerSet = 1,
                SetCount = 1,
            };
        }

        protected override void OnSetPointerPosition(Vector2F position)
        {
            // TODO implement.
        }

        protected override List<InputDeviceFeature> OnInitialize(InputService service)
        {
            List<InputDeviceFeature> baseFeatures = base.OnInitialize(service);
            IsConnected = false;

            return baseFeatures;
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
                        StateSetCount = 5;
                    else if (pm.HasSystemFeature(PackageManager.FeatureTouchscreenMultitouchDistinct))
                        StateSetCount = 2;
                    else
                        StateSetCount = 1;
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
            if (!IsEnabled)
                return;

            // TODO process touch queue and trigger events accordingly.
            // TODO figure out gestures based on number of pressed touch points.
            // TODO individually track each active touch-point so we can form gestures easily.

            PointerState ps = new PointerState();
            ps.SetID = e.Event.ActionIndex;

            switch (e.Event.ActionMasked)
            {
                case MotionEventActions.PointerDown:
                case MotionEventActions.Down:
                    ps.Button = PointerButton.Left;
                    ps.Action = InputAction.Pressed; break;

                case MotionEventActions.PointerUp:
                case MotionEventActions.Up:
                    ps.Button = PointerButton.Left;
                    ps.Action = InputAction.Released;
                    break;

                case MotionEventActions.Move:
                    ps.Action = InputAction.Moved;
                    break;

                // NOTE: A movement has happened outside of the normal bounds of the UI element.
                case MotionEventActions.Outside:
                    ps.Action = InputAction.Moved;
                    break;

                case MotionEventActions.Scroll:
                    ps.Action = InputAction.Moved; 
                    break;
            }

            float pX = e.Event.GetX();
            float pY = e.Event.GetY();
            ps.Position = new Vector2F(pX, pY);
            ps.Pressure = e.Event.GetPressure(ps.SetID);
            ps.Orientation = e.Event.GetOrientation(ps.SetID);
            ps.Size = e.Event.GetSize(ps.SetID);

            // We've handled the touch event
            QueueState(ps);
            e.Handled = true;
        }

        protected override void OnUpdate(Timing time) { }
    }
}
