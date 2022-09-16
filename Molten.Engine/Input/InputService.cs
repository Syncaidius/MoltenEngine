using Molten.Graphics;
using Molten.Threading;

namespace Molten.Input
{
    public abstract class InputService : EngineService
    {
        /// <summary>
        /// Gets or sets the camera through which input is handled. 
        /// If the camera does not have a valid <see cref="INativeSurface"/>, input handling will be skipped.
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

        INativeSurface _activeSurface;
        IInputCamera _activeCamera;

        Dictionary<int, GamepadDevice> _gamepadsByIndex;
        Dictionary<Type, InputDevice> _byType;
        List<InputDevice> _devices;

        /// <summary>Initializes the current input manager instance. Avoid calling this directly unless you know what you are doing.</summary>
        /// <param name="settings">The <see cref="InputSettings"/> that was provided when the engine was instanciated.</param>
        /// <param name="log">A logger.</param>
        protected override void OnInitialize(EngineSettings settings)
        {
            _gamepadsByIndex = new Dictionary<int, GamepadDevice>();
            _byType = new Dictionary<Type, InputDevice>();
            _devices = new List<InputDevice>();
        }

        private void BindSurface(IRenderSurface2D surface)
        {
            if (surface is INativeSurface window)
            {
                // Are we already bound to this surface (e.g. via a different camera).
                if (_activeSurface != window)
                {
                    foreach(InputDevice device in _devices)
                        device.ClearState();

                    if (_activeSurface != null)
                    {
                        foreach (InputDevice device in _devices)
                            device.Unbind(_activeSurface);
                    }

                    _activeSurface = window;

                    if (_activeSurface != null)
                    {
                        foreach (InputDevice device in _devices)
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
                    foreach (InputDevice device in _devices)
                        device.Unbind(_activeSurface);
                }

                _activeSurface = null;
            }

            OnBindSurface(_activeSurface);
        }

        public void ClearState()
        {
            foreach (InputDevice device in _devices)
                device.ClearState();

            OnClearState();
        }

        protected abstract void OnClearState();

        /// <summary>
        /// Gets the default gamepad handler for the current input library.
        /// </summary>
        /// <param name="surface">The window surface the handler will be bound to.</param>
        /// <param name="index">The gamepad player index.</param>
        /// <param name="subtype">The sub-type of gamepad/controller to request. 
        /// Depending on implementation, the returned device may not always match the requested sub-type.</param>
        /// <returns></returns>
        public T GetGamepad<T>(int index, GamepadSubType subtype) where T : GamepadDevice
        {
            GamepadDevice gamepad = null;
            if (!_gamepadsByIndex.TryGetValue(index, out gamepad))
            {
                gamepad = OnGetGamepad(index, subtype);
                _gamepadsByIndex.Add(index, gamepad);
            }

            return gamepad as T;
        }

        /// <summary>Gets a new or existing instance of an input handler for the specified <see cref="INativeSurface"/>.</summary>
        /// <typeparam name="T">The type of handler to retrieve.</typeparam>
        /// <param name="surface">The surface for which to bind and return an input handler.</param>
        /// <returns>An input handler of the specified type.</returns>
        public T GetCustomDevice<T>() where T : InputDevice, new()
        {
            if (!_byType.TryGetValue(typeof(T), out InputDevice device))
            {
                device = new T();
                device.Initialize(this);

                AddDevice(device);

                if (_activeSurface != null)
                    device.Bind(_activeSurface);

                device.OnDisposing += Device_OnDisposing;
            }

            return device as T;
        }

        private void AddDevice(InputDevice device)
        {
            Type dType = typeof(InputDevice);
            Type t = device.GetType();

            // Add the device to all relevant device type categories.
            _devices.Add(device);
            while (dType.IsAssignableFrom(t))
            {
                _byType.Add(t, device);

                t = t.BaseType;
                if (t == dType)
                    break;
            }
        }

        private void RemoveDevice(InputDevice device)
        {
            Type dType = typeof(InputDevice);
            Type t = device.GetType();

            _devices.Remove(device);
            while (dType.IsAssignableFrom(t))
            {
                _byType.Remove(t);

                t = t.BaseType;
                if (t == dType)
                    break;
            }
        }

        private void Device_OnDisposing(EngineObject obj)
        {
            InputDevice device = obj as InputDevice;
            RemoveDevice(device);
        }

        /// <summary>Update's the current input manager. Avoid calling directly unless you know what you're doing.</summary>
        /// <param name="time">An instance of timing for the current thread.</param>
        protected override void OnUpdate(Timing time)
        {
            UpdateID++;

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
        }

        protected override ThreadingMode OnStart(ThreadManager threadManager)
        {
            return ThreadingMode.MainThread;
        }

        protected override void OnStop() { }

        protected override void OnDispose()
        {
            foreach (InputDevice device in _byType.Values)
                device.Dispose();

            _byType.Clear();
        }

        protected abstract void OnBindSurface(INativeSurface surface);

        protected abstract GamepadDevice OnGetGamepad(int index, GamepadSubType subtype);

        /// <summary>
        /// Gets the default mouse handler for the current <see cref="IInputManager"/>.
        /// </summary>
        /// <returns></returns>
        public abstract MouseDevice GetMouse();

        /// <summary>
        /// Gets the default keyboard device handler for the current <see cref="IInputManager"/>.
        /// </summary>
        /// <returns></returns>
        public abstract KeyboardDevice GetKeyboard();

        /// <summary>
        /// Gets the default touch device handler for the current <see cref="IInputManager"/>.
        /// </summary>
        /// <returns></returns>
        public abstract TouchDevice GetTouch();


        /// <summary>Gets the implementation of <see cref="IClipboard"/> bound to the current input manager.</summary>
        public abstract IClipboard Clipboard { get; }

        public abstract IInputNavigation Navigation { get; }

        /// <summary>
        /// Gets the input update/frame ID. 
        /// This is usually equal to the number of times <see cref="InputService.Update(Timing)"/> has been called.
        /// </summary>
        public uint UpdateID { get; private set; }
    }
}
