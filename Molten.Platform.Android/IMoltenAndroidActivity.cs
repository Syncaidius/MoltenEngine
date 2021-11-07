using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Molten.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten
{
    /// <summary>
    /// An abstract implementation of an Android <see cref="Activity"/> that is compatible for use with Molten.
    /// </summary>
    public interface IMoltenAndroidActivity
    {
        /// <summary>
        /// Triggered when a back button is pressed.
        /// </summary>
        event MoltenEventHandler<IMoltenAndroidActivity> BackPressed;


        /// <summary>Invoked when the activity receives a new activity result.</summary>
        event ActivityResultHandler OnResult;

        /// <summary>
        /// Gets the underlying activity for the current <see cref="IMoltenAndroidActivity"/> implementation.
        /// </summary>
        Activity UnderlyingActivity { get; }
    }
}