using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Molten.Input
{
    public abstract class InputDeviceBase : EngineObject , IInputDevice
    {
        protected IInputManager _manager;

        internal virtual void Initialize(IInputManager manager, Logger log)
        {
            _manager = manager;
        }

        public abstract void ClearState();

        /// <summary>Occurs when the device is to bind to the provided surface.</summary>
        /// <param name="surface">The surface that the device should bind to.</param>
        internal abstract void Bind(IWindowSurface surface);

        /// <summary>Occurs when the device is to unbind from the provided surface.</summary>
        /// <param name="surface">The surface from which the device should unbind.</param>
        internal abstract void Unbind(IWindowSurface surface);

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

        /// <summary>
        /// Gets the handle of the parent window.
        /// </summary>
        /// <returns></returns>
        private protected IntPtr? GetWindowHandle(IWindowSurface surface)
        {
            // Check if the surface handle is a form. 
            // If not, find it's parent form.
            Control ctrl = Control.FromHandle(surface.Handle);
            if (ctrl is Form frm)
            {
                return surface.Handle;
            }
            else
            {
                while (ctrl != null)
                {
                    frm = ctrl as Form;
                    if (frm == null)
                        ctrl = ctrl.Parent;
                    else
                        return frm.Handle;
                }
            }

            return null;
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

        /// <summary>Returns true if the specified button was pressed in both the previous and current frame. </summary>
        /// <param name="value">The button or key to check.</param>
        /// <returns></returns>
        public abstract bool IsHeld(T value);

    }
}
