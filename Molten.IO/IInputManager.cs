using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.IO
{
    public interface IInputManager
    {
        /// <summary>Initializes the current input manager instance. Avoid calling this directly unless you know what you are doing.</summary>
        /// <param name="settings">The <see cref="InputSettings"/> that was provided when the engine was instanciated.</param>
        /// <param name="log">A logger.</param>
        void Initialize(InputSettings settings, Logger log);

        /// <summary>Gets a new or existing instance of an input handler for the specified <see cref="IWindowSurface"/>.</summary>
        /// <typeparam name="T">The type of handler to retrieve.</typeparam>
        /// <param name="surface">The surface for which to bind and return an input handler.</param>
        /// <returns>An input handler of the specified type.</returns>
        T GetHandler<T>(IWindowSurface surface) where T : InputHandlerBase, new();

        /// <summary>Sets the active/focused <see cref="IWindowSurface"/> which will receive input. Only one can receive input at any one time.</summary>
        /// <param name="surface">The surface to be set as active.</param>
        void SetActiveWindow(IWindowSurface surface);

        /// <summary>Update's the current input manager. Avoid calling directly unless you know what you're doing.</summary>
        /// <param name="time">An instance of timing for the current thread.</param>
        void Update(Timing time);

        /// <summary>Gets the implementation of <see cref="IClipboard"/> bound to the current input manager.</summary>
        IClipboard Clipboard { get; }
    }
}
