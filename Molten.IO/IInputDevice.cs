using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.IO
{
    public delegate void InputConnectionHandler<T>(IInputDevice<T> device, bool isConnected) where T : struct;

    public interface IInputDevice
    {
        /// <summary>Attempts to open the associated control pane application/software for the device. Does nothing if no control app is available.</summary>
        void OpenControlPanel();

        /// <summary>
        /// Clears the current state of the input handler.
        /// </summary>
        void ClearState();

        /// <summary>Gets whether or not the input device is connected.</summary>
        bool IsConnected { get; }

        /// <summary>Gets the name of the device.</summary>
        string DeviceName { get; }
    }

    public interface IInputDevice<T> where T: struct
    {
        event InputConnectionHandler<T> OnConnectionStatusChanged;

        /// <summary>Returns true if the specified button is pressed.</summary>
        /// <param name="value">The button or key to check.</param>
        /// <returns>Returns true if the button is pressed.</returns>
        bool IsPressed(T value);

        /// <summary>Returns true if any of the provided keys/buttons are pressed.</summary>
        /// <param name="values">The buttons or keys to check.</param>
        /// <returns></returns>
        bool IsAnyPressed(params T[] values);

        /// <summary>Returns true if the specified button was tapped.</summary>
        /// <param name="value">The button or key to check.</param>
        /// <returns></returns>
        bool IsTapped(T value);

        /// <summary>Returns true if the specified button is being held down. </summary>
        /// <param name="value">The button or key to check.</param>
        /// <param name="interval">The interval of time the button(s) must be held for to be considered as held.</param>
        /// <param name="reset">Set to true if the current amount of time the button has been held should be reset.</param>
        /// <returns></returns>
        bool IsHeld(T value, int interval, bool reset);
    }
}
