using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Input
{
    /// <summary>
    /// A helper base class for implementing <see cref="IInputDevice"/>.
    /// </summary>
    public abstract class WinInputDeviceBase : InputDevice
    {
        internal virtual void Initialize(WinInputManager manager, Logger log)
        {
            Manager = manager;
        }

        public abstract void ClearState();



        internal abstract void Update(Timing time);

        /// <summary>Attempts to open the associated control pane application for the device. 
        /// Does nothing if no control app is available.</summary>
        public abstract void OpenControlPanel();

        /// <summary>Gets whether or not the input device is connected.</summary>
        public abstract bool IsConnected { get; }

        /// <summary>Gets the name of the device.</summary>
        public abstract string DeviceName { get; }

        /// <summary>Gets the <see cref="IInputManager"/> that the current input device is bound to.</summary>
        public IInputManager Manager { get; private set; }
    }

    /// <summary>
    /// A helper base class for implementing <see cref="IInputDevice{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class WinInputDeviceBase<T> : WinInputDeviceBase, IInputDevice<T> where T : struct
    {
        public event InputConnectionStatusHandler<T> OnConnectionStatusChanged;

        public event InputConnectionHandler<T> OnConnected;

        public event InputConnectionHandler<T> OnDisconnected;

        protected void InvokeConnectionStatus(bool isConnected)
        {
            OnConnectionStatusChanged?.Invoke(this, isConnected);
        }

        protected void InvokeOnConnected()
        {
            OnConnected?.Invoke(this);
        }

        protected void InvokeOnDisconnected()
        {
            OnDisconnected?.Invoke(this);
        }

        /// <summary>Returns true if the specified button is pressed.</summary>
        /// <param name="value">The button or key to check.</param>
        /// <returns>Returns true if the button is pressed.</returns>
        public abstract bool IsDown(T value);

        /// <summary>Returns true if any of the provided keys/buttons are pressed.</summary>
        /// <param name="values">The buttons or keys to check.</param>
        /// <returns></returns>
        public bool IsAnyDown(params T[] values)
        {
            if (values == null)
                return false;

            for (int i = 0; i < values.Length; i++)
            {
                if (IsDown(values[i]))
                    return true;
            }

            return false;
        }

        /// <summary>Returns true if the specified button was tapped.</summary>
        /// <param name="value">The button or key to check.</param>
        /// <returns></returns>
        public abstract bool IsTapped(T value);

        /// <summary>Returns true if the specified button was pressed in both the previous and current frame. </summary>
        /// <param name="value">The button or key to check.</param>
        /// <returns></returns>
        public abstract bool IsHeld(T value);
    }
}
