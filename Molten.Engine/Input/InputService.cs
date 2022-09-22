using Molten.Graphics;
using Molten.Threading;

namespace Molten.Input
{
    public abstract class InputService : EngineService
    {
        INativeSurface _activeSurface;

        Dictionary<int, GamepadDevice> _gamepadsByIndex;
        Dictionary<Type, InputDevice> _byType;
        List<InputDevice> _devices;

        /// <summary>
        /// Gets or sets the <see cref="INativeSurface"/> through which input is handled. 
        /// </summary>
        public INativeSurface Surface
        {
            get => _activeSurface;
            set
            {
                if (_activeSurface != value)
                {
                    _activeSurface = value;
                    BindSurface(_activeSurface);
                }
            }
        }

        /// <summary>Initializes the current input manager instance. Avoid calling this directly unless you know what you are doing.</summary>
        /// <param name="settings">The initial engine settings provided on startup.</param>
        protected override void OnInitialize(EngineSettings settings)
        {
            _gamepadsByIndex = new Dictionary<int, GamepadDevice>();
            _byType = new Dictionary<Type, InputDevice>();
            _devices = new List<InputDevice>();
        }

        private void BindSurface(INativeSurface surface)
        {
            // Are we already bound to this surface (e.g. via a different camera).
            if (_activeSurface != surface)
            {
                foreach(InputDevice device in _devices)
                    device.ClearState();

                if (_activeSurface != null)
                {
                    foreach (InputDevice device in _devices)
                        device.Unbind(_activeSurface);
                }

                _activeSurface = surface;

                if (_activeSurface != null)
                {
                    foreach (InputDevice device in _devices)
                        device.Bind(_activeSurface);
                }
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

        /// <summary>
        /// Invoked when the current <see cref="InputService"/> is required to bind to a new <see cref="INativeSurface"/>.
        /// </summary>
        /// <param name="surface"></param>
        protected abstract void OnBindSurface(INativeSurface surface);

        /// <summary>
        /// Invoked when a gamepad for a specific player index is requested.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="subtype"></param>
        /// <returns></returns>
        protected abstract GamepadDevice OnGetGamepad(int index, GamepadSubType subtype);

        /// <summary>
        /// Gets the default mouse handler for the current <see cref="InputService"/>.
        /// </summary>
        /// <returns></returns>
        public abstract MouseDevice GetMouse();

        /// <summary>
        /// Gets the default keyboard device handler for the current <see cref="InputService"/>.
        /// </summary>
        /// <returns></returns>
        public abstract KeyboardDevice GetKeyboard();

        /// <summary>
        /// Gets the default touch device handler for the current <see cref="InputService"/>.
        /// </summary>
        /// <returns></returns>
        public abstract TouchDevice GetTouch();


        /// <summary>Gets the implementation of <see cref="IClipboard"/> bound to the current input manager.</summary>
        public abstract IClipboard Clipboard { get; }

        /// <summary>
        /// Gets navigational input controls, if any.
        /// </summary>
        public abstract IInputNavigation Navigation { get; }

        /// <summary>
        /// Gets the input update/frame ID. 
        /// This is usually equal to the number of times <see cref="EngineService.Update(Timing)"/> has been called on the current <see cref="InputService"/>.
        /// </summary>
        public uint UpdateID { get; private set; }
    }
}
