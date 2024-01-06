using Android.Views;
using Molten.Utility;

namespace Molten;

/// <summary>
/// An abstract implementation of an Android <see cref="Activity"/> that is compatible for use with Molten.
/// </summary>
public interface IMoltenAndroidActivity
{
    /// <summary>
    /// Triggered when a back button is pressed.
    /// </summary>
    event MoltenEventHandler<IMoltenAndroidActivity> BackPressed;

    event MoltenEventHandler<View> OnTargetViewChanged;


    /// <summary>Invoked when the activity receives a new activity result.</summary>
    event ActivityResultHandler OnResult;

    /// <summary>
    /// Gets the underlying activity for the current <see cref="IMoltenAndroidActivity"/> implementation.
    /// </summary>
    Activity UnderlyingActivity { get; }

    /// <summary>
    /// Gets the view that the a Molten engine instance should render to.
    /// </summary>
    View TargetView { get; }
}