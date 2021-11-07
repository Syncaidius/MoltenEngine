using Android.App;
using Android.Views;
using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public class AndroidInputManager : EngineObject, IInputManager
    {
        Logger _log;
        INativeSurface _activeSurface;
        IInputCamera _activeCamera;
        AndroidInputNavigation _navigation;

        Dictionary<Type, AndroidInputDeviceBase> _byType = new Dictionary<Type, AndroidInputDeviceBase>();
        List<AndroidInputDeviceBase> _devices = new List<AndroidInputDeviceBase>();

        public IClipboard Clipboard => throw new NotImplementedException();

        /// <summary>
        /// Gets or sets the current input camera, through which all input is received.
        /// </summary>
        public IInputCamera Camera
        {
            get => _activeCamera;
            set
            {
                if (_activeCamera != value)
                {
                    _activeCamera = value;
                    BindSurface(value?.OutputSurface);
                }
            }
        }

        public IInputNavigation Navigation => _navigation;

        public void Initialize(InputSettings settings, Logger log)
        {
            _log = log;
            _navigation = new AndroidInputNavigation();
        }

        private void BindSurface(IRenderSurface surface)
        {
            if (surface is INativeSurface window)
            {
                // Are we already bound to this surface (e.g. via a different camera).
                if (_activeSurface != window)
                {
                    if (_activeSurface != null)
                    {
                        foreach (AndroidInputDeviceBase device in _devices)
                        {
                            device.ClearState();
                            device.Unbind(_activeSurface);
                        }
                    }

                    _activeSurface = window;

                    if (_activeSurface != null)
                    {
                        foreach (AndroidInputDeviceBase device in _devices)
                            device.Bind(_activeSurface);
                    }
                }
            }
            else
            {
                // if active surface isn't null, we were previously bound to something which was an IWindowSurface.
                // We know the new surface is not IWindowSurface, so unbind.
                if (_activeSurface != null)
                {
                    foreach (AndroidInputDeviceBase device in _devices)
                        device.Unbind(_activeSurface);
                }

                _activeSurface = null;
            }
        }

        internal void ClearState()
        {
            foreach (AndroidInputDeviceBase device in _devices)
                device.ClearState();
        }

        public T GetCustomDevice<T>() where T : class, IInputDevice, new()
        {
            Type t = typeof(T);
            if (_byType.TryGetValue(t, out AndroidInputDeviceBase device))
            {
                return device as T;
            }
            else
            {
                device = new T() as AndroidInputDeviceBase;
                AddDevice(device);
                return device as T;
            }
        }

        internal void AddDevice(AndroidInputDeviceBase device)
        {
            Type t = device.GetType();
            device.Initialize(this, _log);

            if (_activeSurface != null)
                device.Bind(_activeSurface);

            device.OnDisposing += Device_OnDisposing;
            _byType.Add(t, device);
            _devices.Add(device);
        }

        private void Device_OnDisposing(EngineObject obj)
        {
            AndroidInputDeviceBase handler = obj as AndroidInputDeviceBase;
            _devices.Remove(handler);
            _byType.Remove(obj.GetType());
        }

        public IGamepadDevice GetGamepad(GamepadIndex index)
        {
            throw new NotImplementedException();
        }

        public IKeyboardDevice GetKeyboard()
        {
            throw new NotImplementedException();
        }

        public IMouseDevice GetMouse()
        {
            throw new NotImplementedException();
        }

        public ITouchDevice GetTouch()
        {
            return GetCustomDevice<AndroidTouchDevice>();
        }

        T IInputManager.GetCustomDevice<T>()
        {
            throw new NotImplementedException();
        }

        /// <summary>Update's the current input manager. Avoid calling directly unless you know what you're doing.</summary>
        /// <param name="time">An instance of timing for the current thread.</param>
        public void Update(Timing time)
        {
            if (_activeSurface != null)
            {
                for (int i = 0; i < _devices.Count; i++)
                    _devices[i].Update(time);
            }
            else
            {
                for (int i = 0; i < _devices.Count; i++)
                    _devices[i].ClearState();
            }

            _navigation.Update(time);
        }

        protected override void OnDispose()
        {
            foreach (AndroidInputDeviceBase device in _byType.Values)
                device.Dispose();

            _byType.Clear();
        }
    }
}
