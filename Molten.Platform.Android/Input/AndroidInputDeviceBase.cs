using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public abstract class AndroidInputDeviceBase : EngineObject, IInputDevice
    {
        public abstract bool IsConnected { get; protected set; }

        public abstract string DeviceName { get; }

        internal virtual void Initialize(AndroidInputManager manager, Logger log)
        {
            Manager = manager;
        }

        /// <summary>Occurs when the device is to bind to the provided surface.</summary>
        /// <param name="surface">The surface that the device should bind to.</param>
        internal abstract void Bind(INativeSurface surface);

        /// <summary>Occurs when the device is to unbind from the provided surface.</summary>
        /// <param name="surface">The surface from which the device should unbind.</param>
        internal abstract void Unbind(INativeSurface surface);

        internal abstract void Update(Timing time);

        public abstract void ClearState();

        public abstract void OpenControlPanel();

        public IInputManager Manager { get; private set; }
    }

    public abstract class AndroidInputDeviceBase<T> : AndroidInputDeviceBase, IInputDevice<T>
        where T : struct
    {
        public event InputConnectionStatusHandler<T> OnConnectionStatusChanged;
        public event InputConnectionHandler<T> OnConnected;
        public event InputConnectionHandler<T> OnDisconnected;

        public bool IsAnyPressed(params T[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsHeld(T value)
        {
            throw new NotImplementedException();
        }

        public bool IsPressed(T value)
        {
            throw new NotImplementedException();
        }

        public bool IsTapped(T value)
        {
            throw new NotImplementedException();
        }
    }
}
