using SharpDX.DirectInput;
using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Input
{
    public class InputManager : EngineObject, IInputManager
    {
        DirectInput _input;

        Logger _log;

        List<GamepadDevice> _gamepads;
        INativeSurface _activeSurface;
        IInputCamera _activeCamera;
        WindowsClipboard _clipboard;

        Dictionary<GamepadIndex, GamepadDevice> _gamepadsByIndex;
        Dictionary<Type, InputDeviceBase> _byType = new Dictionary<Type, InputDeviceBase>();
        List<InputDeviceBase> _devices = new List<InputDeviceBase>();

        /// <summary>Initializes the current input manager instance. Avoid calling this directly unless you know what you are doing.</summary>
        /// <param name="settings">The <see cref="InputSettings"/> that was provided when the engine was instanciated.</param>
        /// <param name="log">A logger.</param>
        public void Initialize(InputSettings settings, Logger log)
        {
            _log = log;
            _input = new DirectInput();
            _gamepads = new List<GamepadDevice>();
            _gamepadsByIndex = new Dictionary<GamepadIndex, GamepadDevice>();
            _clipboard = new WindowsClipboard();
        }


        public T GetCustomDevice<T>() where T : class, IInputDevice, new()
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
            device.Initialize(this, _log);

            if(_activeSurface != null)
                device.Bind(_activeSurface);

            device.OnDisposing += Device_OnDisposing;
            _byType.Add(t, device);
            _devices.Add(device);
        }

        private void Device_OnDisposing(EngineObject obj)
        {
            InputDeviceBase handler = obj as InputDeviceBase;
            _devices.Remove(handler);
            _byType.Remove(obj.GetType());
        }

        public IMouseDevice GetMouse()
        {
            return GetCustomDevice<MouseDevice>();
        }

        public IKeyboardDevice GetKeyboard()
        {
            return GetCustomDevice<KeyboardDevice>();
        }

        public IGamepadDevice GetGamepad(GamepadIndex index)
        {
            GamepadDevice gamepad = null;
            if (!_gamepadsByIndex.TryGetValue(index, out gamepad))
            {
                gamepad = new GamepadDevice(index);
                _gamepadsByIndex.Add(index, gamepad);
                _gamepads.Add(gamepad);
                AddDevice(gamepad);
            }

            return gamepad;
        }

        /// <summary>Update's the current input manager. Avoid calling directly unless you know what you're doing.</summary>
        /// <param name="time">An instance of timing for the current thread.</param>
        public void Update(Timing time)
        {
            if (_activeSurface != null)
            {
                for(int i = 0; i < _devices.Count; i++)
                    _devices[i].Update(time);

                for (int i = 0; i < _gamepads.Count; i++)
                    _gamepads[i].Update(time);
            }
            else
            {
                for (int i = 0; i < _devices.Count; i++)
                    _devices[i].ClearState();
            }
        }

        /// <summary>Retrieves a gamepad handler.</summary>
        /// <param name="index">The index of the gamepad.</param>
        /// <returns></returns>
        public GamepadDevice GetGamepadHandler(GamepadIndex index)
        {
            return _gamepads[(int)index];
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
                        foreach (InputDeviceBase device in _devices)
                        {
                            device.ClearState();
                            device.Unbind(_activeSurface);
                        }
                    }

                    _activeSurface = window;

                    if (_activeSurface != null)
                    {
                        foreach (InputDeviceBase device in _devices)
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
                    foreach (InputDeviceBase device in _devices)
                        device.Unbind(_activeSurface);
                }

                _activeSurface = null;
            }
        }

        internal void ClearState()
        {
            foreach (InputDeviceBase device in _devices)
                device.ClearState();
        }

        protected override void OnDispose()
        {
            foreach (InputDeviceBase device in _byType.Values)
                device.Dispose();

            _byType.Clear();

            DisposeObject(ref _input);

            for (int i = 0; i < _gamepads.Count; i++)
            {
                GamepadDevice gph = _gamepads[i];
                DisposeObject(ref gph);
            }
            _gamepads.Clear();
            _gamepadsByIndex.Clear();
        }

        public DirectInput DirectInput { get { return _input; } }

        /// <summary>Gets the handler for the gamepad at GamepadIndex.One.</summary>
        public GamepadDevice GamePad { get { return _gamepads[0]; } }

        public IClipboard Clipboard => _clipboard;

        /// <summary>
        /// Gets or sets the current input camera, through which all input is received.
        /// </summary>
        public IInputCamera Camera
        {
            get => _activeCamera;
            set
            {
                if(_activeCamera != value)
                {
                    _activeCamera = value;
                    BindSurface(value?.OutputSurface);
                }
            }
        }
    }
}
