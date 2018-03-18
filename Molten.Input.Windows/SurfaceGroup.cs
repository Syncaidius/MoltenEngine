using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    internal class SurfaceGroup : IDisposable
    {
        Dictionary<Type, InputDeviceBase> _byType = new Dictionary<Type, InputDeviceBase>();
        List<InputDeviceBase> _handlers = new List<InputDeviceBase>();

        InputManager _manager;
        Logger _log;
        IWindowSurface _surface;

        internal SurfaceGroup(InputManager manager, Logger log, IWindowSurface surface)
        {
            _manager = manager;
            _log = log;
            _surface = surface;
        }

        internal T GetDevice<T>() where T : class, IInputDevice, new()
        {
            Type t = typeof(T);
            if (_byType.TryGetValue(t, out InputDeviceBase device))
            {
                return device as T;
            }
            else
            {
                device = new T() as InputDeviceBase;
                AddDevice(device);
                return device as T;
            }
        }

        internal void AddDevice(InputDeviceBase device)
        {
            Type t = device.GetType();
            device.Initialize(_manager, _log, _surface);
            device.OnDisposing += Device_OnDisposing;
            _byType.Add(t, device);
            _handlers.Add(device);
        }

        private void Device_OnDisposing(EngineObject obj)
        {
            InputDeviceBase handler = obj as InputDeviceBase;
            _handlers.Remove(handler);
            _byType.Remove(obj.GetType());
        }

        internal void Update(Timing time)
        {
            foreach (InputDeviceBase device in _handlers)
                device.Update(time);
        }

        internal void ClearState()
        {
            foreach (InputDeviceBase device in _handlers)
                device.ClearState();
        }

        public void Dispose()
        {
            foreach (InputDeviceBase device in _byType.Values)
                device.Dispose();

            _byType.Clear();
            _log = null;
            _manager = null;
        }
    }
}
