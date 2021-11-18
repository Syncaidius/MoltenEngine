using Molten.Graphics;
using Molten.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public delegate void InputConnectionStatusHandler(InputDevice device, bool isConnected);

    public abstract class InputDevice : EngineObject
    {
        /// <summary>
        /// Invoked when the device is connected.
        /// </summary>
        public event MoltenEventHandler<InputDevice> OnConnected;

        /// <summary>
        /// Invoked when the device is disconnected.
        /// </summary>
        public event MoltenEventHandler<InputDevice> OnDisconnected;

        /// <summary>
        /// Invoked when the connection status of the device has changed.
        /// </summary>
        public event InputConnectionStatusHandler OnConnectionStatusChanged;

        /// <summary>Gets the name of the device.</summary>
        public abstract string DeviceName { get; protected set; }

        /// <summary>
        /// Clears the current state of the input handler.
        /// </summary>
        public void ClearState()
        {
            OnClearState();

            foreach (InputDeviceFeature f in _features)
                f.ClearState();        }

        protected abstract void OnClearState();

        /// <summary>Attempts to open the associated control pane application/software for the device.
        /// Does nothing if no control app is available.</summary>
        public abstract void OpenControlPanel();

        /// <summary>Occurs when the device is to bind to the provided surface.</summary>
        /// <param name="surface">The surface that the device should bind to.</param>
        internal void Bind(INativeSurface surface)
        {
            OnBind(surface);
        }

        protected abstract void OnBind(INativeSurface surface);

        /// <summary>Occurs when the device is to unbind from the provided surface.</summary>
        /// <param name="surface">The surface from which the device should unbind.</param>
        internal void Unbind(INativeSurface surface)
        {

        }

        protected abstract void OnUnbind(INativeSurface surface);

        public IInputManager Manager { get; }

        bool _connected;
        List<InputDeviceFeature> _features;

        public InputDevice(IInputManager manager, Logger log)
        {
            Manager = manager;
            Log = log;
            _features = Initialize();
            Features = _features.AsReadOnly();
        }

        /// <summary>
        /// Invoked during device initialization to allow the device to initialize and define it's feature-set.
        /// </summary>
        /// <param name="features"></param>
        /// <returns>A list of detected <see cref="InputDeviceFeature"/> objects.</returns>
        protected abstract List<InputDeviceFeature> Initialize();

        protected void LogFeatures()
        {
#if DEBUG
            Log.WriteLine($"Initialized device '{DeviceName}' with {_features.Count} features: ");
            foreach (InputDeviceFeature feature in _features)
            {
                Log.WriteLine($"   {feature.Name} - {feature.Description}");
            }
#endif
        }

        public bool HasFeature<T>() where T : InputDeviceFeature
        {
            return GetFeature<T>() != null;
        }

        /// <summary>
        /// Retrieves a device feature of the specified type, or null if the feature is unsupported.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetFeature<T>() where T: InputDeviceFeature
        {
            Type t = typeof(T);

            foreach (InputDeviceFeature f in _features)
            {
                if (t.IsAssignableFrom(f.GetType()))
                    return f as T;
            }

            return null;
        }

        /// <summary>
        /// Gets a list of features bound to the current <see cref="InputDevice"/>.
        /// </summary>
        public IReadOnlyCollection<InputDeviceFeature> Features { get; }

        /// <summary>Gets whether or not the current <see cref="InputDevice"/> is connected.</summary>
        public bool IsConnected
        {
            get => _connected;
            protected set
            {
                if (value != _connected)
                {
                    _connected = value;

                    if (value)
                        OnConnected?.Invoke(this);
                    else
                        OnDisconnected?.Invoke(this);

                    OnConnectionStatusChanged?.Invoke(this, _connected);
                }
            }
        }

        public void Update(Timing time)
        {
            OnUpdate(time);

            foreach (InputDeviceFeature f in _features)
                f.Update(time);
        }

        protected abstract void OnUpdate(Timing time);

        protected Logger Log { get; }
    }

    public abstract class InputDevice<T> : InputDevice
        where T : struct
    {
        public InputDevice(IInputManager manager, Logger log) : base(manager, log)
        {

        }

        /// <summary>Returns true if the specified input is pressed.</summary>
        /// <param name="value">The input (e.g. button or key) to check.</param>
        /// <returns>Returns true if the input is down.</returns>
        public abstract bool IsDown(T value);

        /// <summary>Returns true if any of the provided inputs are pressed.</summary>
        /// <param name="values">The buttons or keys to check.</param>
        /// <returns></returns>
        public abstract bool IsAnyDown(params T[] values);

        /// <summary>Returns true if the spcified input was just pressed/down, but was not in the last update tick.</summary>
        /// <param name="value">The button or key to check.</param>
        /// <returns></returns>
        public abstract bool IsTapped(T value);

        /// <summary>Returns true if the specified button was pressed in both the previous and current update tick. </summary>
        /// <param name="value">The button or key to check.</param>
        /// <returns></returns>
        public abstract bool IsHeld(T value);
    }
}
