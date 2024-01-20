using Molten.Graphics;
using Molten.Utility;

namespace Molten.Input;

public delegate void InputConnectionStatusHandler(InputDevice device, bool isConnected);

public delegate void InputBufferSizeChangedHandler(InputDevice device, int oldSize, int newSize);

public abstract class InputDevice : EngineObject
{
    protected class StateParameters
    {
        /// <summary>
        /// The number of separate state sets to keep track of. Each state set will track 1 or more states (e.g. buttons). 
        /// This is useful when a device has multiple pointers, each with 1 or more button states to track e.g. fingers or dual-hand pointing device.
        /// </summary>
        public int SetCount { get; set; }

        /// <summary>
        /// The number of states to track per state set.
        /// </summary>
        public int StatesPerSet { get; set; }

        public StateParameters() { }
    }

    /// <summary>
    /// Invoked when the device is (re)connected.
    /// </summary>
    public event MoltenEventHandler<InputDevice> OnConnected;

    /// <summary>
    /// Invoked when the device is disconnected.
    /// </summary>
    public event MoltenEventHandler<InputDevice> OnDisconnected;

    /// <summary>
    /// Invoked when the device is (re)enabled.
    /// </summary>
    public event MoltenEventHandler<InputDevice> OnEnabled;

    /// <summary>
    /// Invoked when the device is disabled.
    /// </summary>
    public event MoltenEventHandler<InputDevice> OnDisabled;

    /// <summary>
    /// Invoked when the connection status of the device has changed.
    /// </summary>
    public event InputConnectionStatusHandler OnConnectionStatusChanged;

    /// <summary>Gets the name of the device.</summary>
    public abstract string DeviceName { get; }

    /// <summary>
    /// Gets the buffer size, in samples, for storing unprocessed device states.
    /// </summary>
    public abstract int BufferSize { get; protected set; }

    /// <summary>
    /// Gets the number of simultaneous states that each state-set can keep track of on the current <see cref="InputDevice"/>.
    /// </summary>
    public abstract int StatesPerSet { get; protected set; }

    /// <summary>
    /// Gets the <see cref="InputService"/> that the current <see cref="InputDevice"/> is bound to.
    /// </summary>
    public InputService Service { get; private set; }

    /// <summary>
    /// Gets a list of features bound to the current <see cref="InputDevice"/>.
    /// </summary>
    public IReadOnlyCollection<InputDeviceFeature> Features { get; private set; }

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

    /// <summary>
    /// Gets or sets whether the current <see cref="InputDevice"/> is enabled. 
    /// If false, the device will stop receiving and processing input, even if <see cref="IsConnected"/> is still true.
    /// <para><see cref="IsEnabled"/> will be false if <see cref="IsConnected"/> is also false, regardless of the internal enabled state.</para>
    /// </summary>
    public bool IsEnabled
    {
        get => _enabled;
        set
        {
            // Don't allow device to be enabled if it was disposed.
            if (IsDisposed)
                value = false;

            if (value != _enabled)
            {
                _enabled = value;
                if (_enabled)
                    OnEnabled?.Invoke(this);
                else
                    OnDisabled?.Invoke(this);
            }
        }
    }

    protected override void OnDispose(bool immediate)
    {
        IsEnabled = false;
    }

    /// <summary>
    /// Gets the number of state sets in the current <see cref="InputDevice"/>. E.g. fingers, mouse pointers or multi-part device count.
    /// </summary>
    public int StateSetCount
    {
        get => _stateParams.SetCount;
        set
        {
            if(_stateParams.SetCount != value)
            {
                _stateParams.SetCount = value;
                InitializeBuffer(_stateParams, BufferSize);
            }
        }
    }

    bool _connected;
    bool _enabled;
    List<InputDeviceFeature> _features;
    StateParameters _stateParams;
    
    internal void Initialize(InputService service) 
    {
        _enabled = true;
        Service = service;

        _stateParams = GetStateParameters();

        SettingValue<int> bufferSizeSetting = GetBufferSizeSetting(service.Settings.Input);
        InitializeBuffer(_stateParams, bufferSizeSetting.Value);
        bufferSizeSetting.OnChanged += BufferSizeSetting_OnChanged;

        _features = OnInitialize(service) ?? new List<InputDeviceFeature>(); 
        Features = _features.AsReadOnly();
    }

    private void BufferSizeSetting_OnChanged(int oldValue, int newValue)
    {
        InitializeBuffer(_stateParams, newValue);
    }

    /// <summary>
    /// Invoked during device initialization to allow the device to initialize and define it's feature-set.
    /// </summary>
    /// <param name="service">The <see cref="InputService"/> that is initializing the current <see cref="InputDevice"/>.</param>
    /// <returns>A list of detected <see cref="InputDeviceFeature"/> objects.</returns>
    protected virtual List<InputDeviceFeature> OnInitialize(InputService service)
    {
        return null;
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

    protected abstract StateParameters GetStateParameters();

    protected abstract SettingValue<int> GetBufferSizeSetting(InputSettings settings);

    protected abstract void InitializeBuffer(StateParameters stateParams, int bufferSize);

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

    protected void LogFeatures()
    {
#if DEBUG
        Service.Log.WriteLine($"Initialized device '{DeviceName}' with {_features.Count} features: ");
        foreach (InputDeviceFeature feature in _features)
            Service.Log.WriteLine($"   {feature.Name} - {feature.Description}");
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
    where S : struct, IInputState
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
    public override sealed int StatesPerSet
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
    S[][] _states;
    int _bStart;
    int _bEnd;

    protected override sealed void InitializeBuffer(StateParameters stateParams, int bufferSize)
    {
        _buffer = new S[bufferSize];

        if (_states == null)
        {
            _states = new S[stateParams.SetCount][];
            for (int i = 0; i < stateParams.SetCount; i++)
                _states[i] = new S[stateParams.StatesPerSet];
        }
    }

    /// <summary>
    /// Queues a state on the input device. This can be used to simulate input.
    /// </summary>
    /// <param name="state">The state to be queued on the device's buffer.</param>
    public void QueueState(S state)
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

        for (int s = 0; s < _states.Length; s++)
        {
            for (int i = 0; i < _states.Length; i++)
                _states[s][i] = new S();
        }

        base.ClearState();
    }

    /// <summary>
    /// Retrieves the state of the given state ID. 
    /// </summary>
    /// <param name="setID">The state set in which the state exists</param>
    /// <param name="state">The button or key to be retrieved.</param>
    /// <returns></returns>
    public S GetState(T state, int setID = 0)
    {
        int stateID = TranslateStateID(state);
        if (stateID > StatesPerSet)
            throw new IndexOutOfRangeException($"stateID was greater than or equal to {nameof(StatesPerSet)}, which is {StatesPerSet}.");

        return _states[setID][stateID];
    }

    internal override void Update(Timing time)
    {
        OnUpdate(time);

        if (!IsEnabled)
            return;

        if (_bStart != _bEnd)
        {
            while (_bStart != _bEnd)
            {
                if (_bStart == _buffer.Length)
                    _bStart = 0;

                S state = _buffer[_bStart];
                state.UpdateID = Service.UpdateID;
                int stateID = GetStateID(ref state);

                S prev = _states[state.SetID][stateID];

                // Replace prev state with new state, if accepted.
                if (ProcessState(ref state, ref prev))
                    _states[state.SetID][stateID] = state;

                _bStart++;
            }
        }
        else
        {
            ProcessIdleState();
        }

        foreach (InputDeviceFeature f in Features)
            f.Update(time);
    }

    public bool IsHeld(T stateID, int setID = 0)
    {
        int id = TranslateStateID(stateID);
        return GetIsHeld(ref _states[setID][id]);
    }

    public bool IsDown(T stateID, int setID = 0)
    {
        int id = TranslateStateID(stateID);
        return GetIsDown(ref _states[setID][id]);
    }

    public bool IsTapped(T stateID, InputActionType type = InputActionType.Single, int setID = 0)
    {
        int id = TranslateStateID(stateID);
        return GetIsTapped(ref _states[setID][id], type);
    }

    /// <summary>
    /// Invoked when the device has no new state updates to process.
    /// </summary>
    protected virtual void ProcessIdleState() { }

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
    /// <param name="state">The input state to check.</param>
    /// <returns>Returns true if the input is down.</returns>
    protected abstract bool GetIsDown(ref S state);

    /// <summary>Returns true if the spcified input was just pressed/down, but was not in the last update tick.</summary>
    /// <param name="state">The button or key to check.</param>
    /// <returns></returns>
    protected abstract bool GetIsTapped(ref S state, InputActionType type);

    /// <summary>Returns true if the specified button was pressed in both the previous and current update tick. </summary>
    /// <param name="state">The button or key to check.</param>
    /// <returns></returns>
    protected abstract bool GetIsHeld(ref S state);
}
