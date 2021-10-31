using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public abstract class AndroidInputDeviceBase : EngineObject, IInputDevice
    {
        public bool IsConnected => throw new NotImplementedException();

        public string DeviceName => throw new NotImplementedException();

        public void ClearState()
        {
            throw new NotImplementedException();
        }

        public void OpenControlPanel()
        {
            throw new NotImplementedException();
        }
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
