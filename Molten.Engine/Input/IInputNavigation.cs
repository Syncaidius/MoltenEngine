using Molten.Utility;

namespace Molten.Input
{
    /// <summary>
    /// Represents an implementation of nagivation controls on the current platform.
    /// </summary>
    public interface IInputNavigation
    {
        /// <summary>
        /// Triggered when a back button is pressed.
        /// </summary>
        event MoltenEventHandler<IInputNavigation> OnBackPressed;

        /// <summary>
        /// Triggered when a context menu button is pressed.
        /// </summary>
        event MoltenEventHandler<IInputNavigation> OnContextButtonPressed;

        /// <summary>
        /// Gets whether or not a back button is pressed.
        /// </summary>
        bool IsBackPressed { get; }

        /// <summary>
        /// Gets whether or not a context menu button is pressed.
        /// </summary>
        bool IsContextButtonPressed { get; }
    }
}
