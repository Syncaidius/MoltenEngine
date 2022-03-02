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

    public delegate void InputBufferSizeChangedHandler(InputDevice device, int oldSize, int newSize);

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
        public abstract string DeviceName { get; }

        public abstract int BufferSize { get; protected set; }

        public InputService Manager { get; }

        /// <summary>
        /// Gets the maximum number of simultaneous states that the current <see cref="InputDevice"/> can keep track of.
        /// </summary>
        public abstract int MaxSimultaneousStates { get; protected set; }

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

        bool _connected;
        List<InputDeviceFeature> _features;

        public InputDevice(InputService manager)
        {
            Manager = manager;
            MaxSimultaneousStates = GetMaxSimultaneousStates();
            _features = Initialize() ?? new List<InputDeviceFeature>();
            Features = _features.AsReadOnly();
        }

        /// <summary>
        /// Clears the current state of the input handler.
        /// </summary>
        public virtual void ClearState()
        {
            OnClearState();

            foreach (InputDeviceFeature f in _features)
                f.ClearState();
        }

        protected abstract int GetMaxSimultaneousStates();

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
            OnUnbind(surface);
        }

        protected abstract void OnUnbind(INativeSurface surface);


        /// <summary>
        /// Invoked during device initialization to allow the device to initialize and define it's feature-set.
        /// </summary>
        /// <param name="features"></param>
        /// <returns>A list of detected <see cref="InputDeviceFeature"/> objects.</returns>
        protected abstract List<InputDeviceFeature> Initialize();

        protected void LogFeatures()
        {
#if DEBUG
            Manager.Log.Log($"Initialized device '{DeviceName}' with {_features.Count} features: ");
            foreach (InputDeviceFeature feature in _features)
                Manager.Log.Log($"   {feature.Name} - {feature.Description}");
#endif
        }

        /// <summary>Returns whether or not the input device supports one or more of the specified <see cref="InputDeviceFeature"/>. </summary>
        /// <typeparam name="T">The type of <see cref="InputDeviceFeature"/> to check.</typeparam>
        /// <param name="nameFilter">If provided, only features of the specified type with a name fully 
        /// or partially-matching the gien name filter will be checked.</param>
        /// <param name="filterCaseSensitive">If true, the name filter will also consider casing.</param>
        /// <returns></returns>
        public bool HasFeature<T>(string nameFilter = null, bool filterCaseSensitive = false) where T : InputDeviceFeature
        {
            return GetFeature<T>(nameFilter, filterCaseSensitive) != null;
        }


        /// <summary>
        /// Retrieves a device feature of the specified type, or null if the feature is unsupported. If a device contains more than one of
        /// the specified <see cref="InputDeviceFeature"/> type, the first one will be returned, based on the order they were detected by the device.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nameFilter">If provided, only features of the specified type with a name/description fully 
        /// or partially-matching the gien name filter will be returned.</param>
        /// <param name="filterCaseSensitive">If true, the name filter will also consider casing.</param>
        /// <returns></returns>
        public T GetFeature<T>(string nameFilter = null, bool filterCaseSensitive = false) where T: InputDeviceFeature
        {
            return GetFeatures<T>(nameFilter, filterCaseSensitive).FirstOrDefault();
        }

        /// <summary>
        /// Retrieve all features of the specified type on the current <see cref="InputDevice"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="InputDeviceFeature"/> to retrieve.</typeparam>
        /// <param name="nameFilter">If provided, only features of the specified type with a name fully 
        /// or partially-matching the gien name filter will be returned.</param>
        /// <param name="filterCaseSensitive">If true, the name filter will also consider casing.</param>
        /// <returns></returns>
        public List<T> GetFeatures<T>(string nameFilter = null, bool filterCaseSensitive = false) where T: InputDeviceFeature
        {
            List<T> list = new List<T>();
            Type t = typeof(T);

            bool shouldFilter = !string.IsNullOrWhiteSpace(nameFilter);
            if (shouldFilter)
            {
                if(!filterCaseSensitive)
                    nameFilter = nameFilter.ToLower();
            }

            foreach (InputDeviceFeature f in _features)
            {
                if (t.IsAssignableFrom(f.GetType()))
                {
                    if (shouldFilter)
                    {
                        string dn = f.Name + f.Description;
                        if (!filterCaseSensitive)
                            dn = dn.ToLower();

                        if (!dn.Contains(nameFilter))
                            continue;
                    }

                    list.Add(f as T);
                }
            }

            return list;
        }

        internal virtual void Update(Timing time)
        {
            OnUpdate(time);

            foreach (InputDeviceFeature f in _features)
                f.Update(time);
        }

        protected abstract void OnUpdate(Timing time);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="S">The type of the state to be stored in the device's buffer.</typeparam>
    /// <typeparam name="T">The input type. Must be an integer-based type. For example a key, button or touch-point enum.</typeparam>
    public abstract class InputDevice<S, T> : InputDevice
        where S : struct
        where T : struct
    {
        /// <summary>
        /// Gets the buffer size of the current <see cref="InputDevice{S, T}"/>
        /// </summary>
        public override int BufferSize
        {
            get => _buffer.Length;
            protected set
            {
                int oldSize = _buffer.Length;
                if (_buffer.Length != value)
                {
                    ClearState();
                    Array.Resize(ref _buffer, value);
                    OnBufferSizeChanged?.Invoke(this, oldSize, value);
                }
            }
        }

        /// <summary>
        /// Gets the maximum number of simultaneous states that the current <see cref="InputDevice"/> can keep track of.
        /// </summary>
        public override sealed int MaxSimultaneousStates
        {
            get => _states.Length;
            protected set
            {
                if(_states == null || _states.Length != value)
                    Array.Resize(ref _states, value);
            }
        }

        public event InputBufferSizeChangedHandler OnBufferSizeChanged;

        S[] _buffer;
        S[] _states;
        int _bStart;
        int _bEnd;

        public InputDevice(InputService manager, int bufferSize) : base(manager)
        {
            _buffer = new S[bufferSize]; 

            if(_states == null)
                _states = new S[5];
        }

        protected void QueueState(S state)
        {
            // Should we circle back to the beginning of the buffer?
            if (_bEnd == _buffer.Length)
                _bEnd = 0;

            _buffer[_bEnd++] = state;
        }

        public override sealed void ClearState()
        {
            _bStart = 0;
            _bEnd = 0;

            for (int i = 0; i < _states.Length; i++)
                _states[i] = new S();

            base.ClearState();
        }

        /// <summary>
        /// Retrieves the state of the given state ID. 
        /// </summary>
        /// <param name="stateID">The state ID. For example, a mouse button, key or touch-point ID.</param>
        /// <returns></returns>
        public S GetState(int stateID)
        {
            if (stateID > MaxSimultaneousStates)
                throw new IndexOutOfRangeException($"stateID was greater than or equal to {nameof(MaxSimultaneousStates)}, which is {MaxSimultaneousStates}.");

            return _states[stateID];
        }

        internal override void Update(Timing time)
        {
            OnUpdate(time);

            while (_bStart != _bEnd)
            {
                if (_bStart == _buffer.Length)
                    _bStart = 0;

                S state = _buffer[_bStart];
                int stateID = GetStateID(ref state);
                S prev = _states[stateID];
                bool accept = ProcessState(ref state, ref prev);

                // Replace state with new state, if accepted.
                if (accept)
                    _states[stateID] = state;

                _bStart++;
            }

            foreach (InputDeviceFeature f in Features)
                f.Update(time);
        }


        public bool IsAnyDown(params T[] stateIDs)
        {
            for (int i = 0; i < stateIDs.Length; i++)
            {
                int id = TranslateStateID(stateIDs[i]);

                if (id > _states.Length)
                    continue;

                if (GetIsDown(ref _states[id]))
                    return true;
            }

            return false;
        }

        public bool IsHeld(T stateID)
        {
            int id = TranslateStateID(stateID);
            return GetIsHeld(ref _states[id]);
        }

        public bool IsDown(T stateID)
        {
            int id = TranslateStateID(stateID);
            return GetIsDown(ref _states[id]);
        }

        public bool IsTapped(T stateID)
        {
            int id = TranslateStateID(stateID);
            return GetIsTapped(ref _states[id]);
        }

        /// <summary>
        /// Invoked when the current <see cref="InputDevice"/> needs to process a state from it's buffer.
        /// </summary>
        /// <param name="newState">The new data provided by an input state.</param>
        /// <param name="prevState">The previous data provided by an input state.</param>
        /// <returns>The ID of the state. For example, a key, touch-point or button ID.</returns>
        protected abstract bool ProcessState(ref S newState, ref S prevState);

        /// <summary>
        /// Invoked when the device needs to retrieve a state's ID during processing.
        /// </summary>
        /// <param name="state">The state of which to retrieve the state ID from.</param>
        /// <returns></returns>
        protected abstract int GetStateID(ref S state);

        /// <summary>
        /// Invoked when a state ID type needs translating into an integer ID.
        /// </summary>
        /// <param name="idValue">The value of the state ID, in the device's native state value type e.g. key or button enum.</param>
        /// <returns></returns>
        protected abstract int TranslateStateID(T idValue);

        /// <summary>Returns true if the specified input is pressed.</summary>
        /// <param name="value">The input (e.g. button or key) to check.</param>
        /// <returns>Returns true if the input is down.</returns>
        protected abstract bool GetIsDown(ref S state);

        /// <summary>Returns true if the spcified input was just pressed/down, but was not in the last update tick.</summary>
        /// <param name="state">The button or key to check.</param>
        /// <returns></returns>
        protected abstract bool GetIsTapped(ref S state);

        /// <summary>Returns true if the specified button was pressed in both the previous and current update tick. </summary>
        /// <param name="state">The button or key to check.</param>
        /// <returns></returns>
        protected abstract bool GetIsHeld(ref S state);
    }
}
