using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Input
{
    public abstract class InputDeviceBase : EngineObject , IInputDevice
    {
        protected IInputManager _manager;

        internal virtual void Initialize(IInputManager manager, Logger log, IWindowSurface surface)
        {
            _manager = manager;
        }

        public abstract void ClearState();

        internal abstract void Update(Timing time);

        /// <summary>Attempts to open the associated control pane application for the device. Does nothing if no control app is available.</summary>
        public abstract void OpenControlPanel();

        /// <summary>Gets whether or not the input device is connected.</summary>
        public abstract bool IsConnected { get; }

        /// <summary>Gets the name of the device.</summary>
        public abstract string DeviceName { get; }
    }

    public abstract class InputHandlerBase<T> : InputDeviceBase, IInputDevice<T> where T : struct
    {
        public event InputConnectionHandler<T> OnConnectionStatusChanged;

        protected void InvokeConnectionStatus(bool isConnected)
        {
            OnConnectionStatusChanged?.Invoke(this, isConnected);
        }
        
        /// <summary>Returns true if the specified button is pressed.</summary>
        /// <param name="value">The button or key to check.</param>
        /// <returns>Returns true if the button is pressed.</returns>
        public abstract bool IsPressed(T value);

        /// <summary>Returns true if any of the provided keys/buttons are pressed.</summary>
        /// <param name="values">The buttons or keys to check.</param>
        /// <returns></returns>
        public bool IsAnyPressed(params T[] values)
        {
            if (values == null)
                return false;

            for (int i = 0; i < values.Length; i++)
            {
                if (IsPressed(values[i]))
                    return true;
            }

            return false;
        }

        /// <summary>Returns true if the specified button was tapped.</summary>
        /// <param name="value">The button or key to check.</param>
        /// <returns></returns>
        public abstract bool IsTapped(T value);

        /// <summary>Returns true if the specified button is being held down. </summary>
        /// <param name="value">The button or key to check.</param>
        /// <param name="interval">The interval of time the button(s) must be held for to be considered as held.</param>
        /// <param name="reset">Set to true if the current amount of time the button has been held should be reset.</param>
        /// <returns></returns>
        public abstract bool IsHeld(T value, int interval, bool reset);

    }
}
