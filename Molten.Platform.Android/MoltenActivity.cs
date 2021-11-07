using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Molten.Graphics;
using Molten.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Platform
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
    public abstract class MoltenActivity : Activity
    {
        View _splash;
        FrameLayout _view;

        /// <summary>Invoked when the activity receives a new activity result.</summary>
        public event ActivityResultHandler OnResult;


        protected override sealed void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _view = new FrameLayout(this.ApplicationContext);
            OnCreateApp(_view);
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
                _view.Dispose();
            }
            base.Dispose(disposing);
        }

        protected abstract void OnCreateApp(FrameLayout view);
    }

    /// <summary>
    /// An Android <see cref="Activity"/> implementation for initializting and using a Molten <see cref="Foundation{R, I}"/>.
    /// </summary>
    public abstract class MoltenActivity<T, R, I> : MoltenActivity
        where T : Foundation<R, I>
        where R : MoltenRenderer, new()
        where I : class, IInputManager, new()
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
            Type tMainType = typeof(Foundation<,>);
            Type[] tGenerics = { typeof(R), typeof(I) };

            Type fType = tMainType.MakeGenericType(tGenerics);
            FoundationInstance = (Foundation<R, I>)Activator.CreateInstance(fType, new object[] { Title });
        }

        /// <summary>
        /// Gets the <see cref="Foundation{R, I}"/> instanced bound to the current <see cref="MoltenActivity{T, R, I}"/>.
        /// </summary>
        public Foundation<R, I> FoundationInstance { get; private set; }
    }
}