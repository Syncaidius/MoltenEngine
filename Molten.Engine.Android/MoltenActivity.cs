using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Molten.Graphics;
using Molten.Input;
using Molten.Utility;
using System;

namespace Molten
{
    public delegate void ActivityResultHandler(int requestCode, [GeneratedEnum] Result resultCode, Intent data);

    [Activity(Label = "@string/app_name"
    , MainLauncher = true
    , Icon = "@drawable/icon"
    , Theme = "@style/Theme.Splash"
    , AlwaysRetainTaskState = true
    , LaunchMode = LaunchMode.SingleInstance
    , ScreenOrientation = ScreenOrientation.SensorPortrait
    , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public abstract class MoltenActivity : Activity, IMoltenAndroidActivity
    {
        View _splash;
        FrameLayout _view;

        /// <summary>
        /// Triggered when a back button is pressed.
        /// </summary>
        public event MoltenEventHandler<IMoltenAndroidActivity> BackPressed;

        public event MoltenEventHandler<View> OnTargetViewChanged;


        /// <summary>Invoked when the activity receives a new activity result.</summary>
        public event ActivityResultHandler OnResult;

        protected override sealed void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _view = new FrameLayout(this.ApplicationContext);
            OnCreateApp(_view);

            SetContentView(_view);
            OnStartApp();
        }

        public override void OnBackPressed()
        {
            BackPressed?.Invoke(this);
            base.OnBackPressed();
        }

        public void ShowSplash()
        {
            if (_splash == null)
                _splash = OnCreateSplashView(_view);

            if (_splash != null)
                _view.AddView(_splash);
        }

        public void HideSplash()
        {
            if (_splash != null)
                _view.RemoveView(_splash);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            OnResult?.Invoke(requestCode, resultCode, data);
            base.OnActivityResult(requestCode, resultCode, data);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BackPressed = null;
                _splash?.Dispose();
                _view.Dispose();
            }
            base.Dispose(disposing);
        }

        protected abstract void OnCreateApp(FrameLayout view);

        protected abstract void OnStartApp();

        protected abstract View OnCreateSplashView(FrameLayout view);

        public Activity UnderlyingActivity => this;

        /// <summary>
        /// Gets the view that the a Molten engine instance should render to.
        /// </summary>
        public View TargetView => null;
    }

    /// <summary>
    /// An Android <see cref="Activity"/> implementation for initializting and using a Molten <see cref="Foundation"/>.
    /// </summary>
    public abstract class MoltenActivity<T> : MoltenActivity
        where T : Foundation
    {
        public MoltenActivity(string initialTitle = "Molten Android App")
        {
            Title = initialTitle;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                FoundationInstance.Dispose();

            base.Dispose(disposing);
        }

        protected override void OnCreateApp(FrameLayout view)
        {
            FoundationInstance = Activator.CreateInstance(typeof(T), new object[] { this.Title }) as T;
        }

        /// <summary>
        /// Gets the <see cref="Foundation"/> instanced bound to the current <see cref="MoltenActivity{T, R, I}"/>.
        /// </summary>
        public T FoundationInstance { get; private set; }
    }
}