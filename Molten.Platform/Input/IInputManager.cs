using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public interface IInputManager
    {
        /// <summary>Initializes the current input manager instance. Avoid calling this directly unless you know what you are doing.</summary>
        /// <param name="settings">The <see cref="InputSettings"/> that was provided when the engine was instanciated.</param>
        /// <param name="log">A logger.</param>
        void Initialize(InputSettings settings, Logger log);

        /// <summary>Gets a new or existing instance of an input handler for the specified <see cref="INativeSurface"/>.</summary>
        /// <typeparam name="T">The type of handler to retrieve.</typeparam>
        /// <param name="surface">The surface for which to bind and return an input handler.</param>
        /// <returns>An input handler of the specified type.</returns>
        T GetCustomDevice<T>() where T : class, IInputDevice, new();

        /// <summary>
        /// Gets the default mouse handler for the current <see cref="IInputManager"/>.
        /// </summary>
        /// <returns></returns>
        IMouseDevice GetMouse();

        /// <summary>
        /// Gets the default keyboard device handler for the current <see cref="IInputManager"/>.
        /// </summary>
        /// <returns></returns>
        IKeyboardDevice GetKeyboard();

        /// <summary>
        /// Gets the default touch device handler for the current <see cref="IInputManager"/>.
        /// </summary>
        /// <returns></returns>
        ITouchDevice GetTouch();

        /// <summary>
        /// Gets the default gamepad handler for the current input library.
        /// </summary>
        /// <param name="surface">The window surface the handler will be bound to.</param>
        /// <param name="index">The gamepad index.</param>
        /// <returns></returns>
        IGamepadDevice GetGamepad(GamepadIndex index);

        /// <summary>Update's the current input manager. Avoid calling directly unless you know what you're doing.</summary>
        /// <param name="time">An instance of timing for the current thread.</param>
        void Update(Timing time);

        /// <summary>Gets the implementation of <see cref="IClipboard"/> bound to the current input manager.</summary>
        IClipboard Clipboard { get; }

        /// <summary>
        /// Gets or sets the camera through which input is handled. If the camera does not have a valid <see cref="INativeSurface"/>, input handling will be skipped.
        /// </summary>
        IInputCamera Camera { get; set; }

        IInputNavigation Navigation { get; }
    }
}
