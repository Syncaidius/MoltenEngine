using Silk.NET.DXGI;

namespace Molten.Graphics.Dxgi
{
    public unsafe delegate GraphicsCapabilities DXGIDetectCapabilitiesCallback(IDXGIAdapter4* adapter);

    public unsafe class DisplayManagerDXGI : EngineObject, IDisplayManager
    {
        const uint DXGI_CREATE_FACTORY_NODEBUG = 0x0;
        const uint DXGI_CREATE_FACTORY_DEBUG = 0x01;

        DXGI _api;
        IDXGIFactory7* _dxgiFactory;
        List<DisplayAdapterDXGI> _adapters;
        List<DisplayAdapterDXGI> _withOutputs;

        DisplayAdapterDXGI _defaultAdapter;
        DisplayAdapterDXGI _selectedAdapter;
        DXGIDetectCapabilitiesCallback _capabilitiesCallback;

        public DisplayManagerDXGI(DXGIDetectCapabilitiesCallback capabilitiesCallback)
        {
            _api = DXGI.GetApi();
            _capabilitiesCallback = capabilitiesCallback; 
            _adapters = new List<DisplayAdapterDXGI>();
            Adapters = _adapters.AsReadOnly();
            _withOutputs = new List<DisplayAdapterDXGI>();
            AdaptersWithOutputs = _withOutputs.AsReadOnly();
        }

        protected override void OnDispose()
        {
            SilkUtil.ReleasePtr(ref _dxgiFactory);
            _api.Dispose();
        }

        /// <summary>
        /// Initializes the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void Initialize(Logger logger, GraphicsSettings settings)
        {
            Log = logger;

            // Create factory
            Guid factoryGuid = IDXGIFactory2.Guid;
            void* ptrFactory = null;
            uint debugFlag = settings.EnableDebugLayer ? DXGI_CREATE_FACTORY_DEBUG : DXGI_CREATE_FACTORY_NODEBUG;

            int r = _api.CreateDXGIFactory2(debugFlag, &factoryGuid, &ptrFactory);
            DxgiError err = DXGIHelper.ErrorFromResult(r);
            if (err != DxgiError.Ok)
            {
                logger.Error($"Failed to initialize DXGI: {err}");
                return;
            }

            IDXGIFactory2* tmp = (IDXGIFactory2*)ptrFactory;
            Guid factory7Guid = IDXGIFactory7.Guid;

            r = tmp->QueryInterface(&factory7Guid, &ptrFactory);
            err = DXGIHelper.ErrorFromResult(r);
            if (err != DxgiError.Ok)
            {
                logger.Error($"Failed to query DXGI 1.6 factory: {err}");
                return;
            }

            _dxgiFactory = (IDXGIFactory7*)ptrFactory;

            // Detect adapters.
            IDXGIAdapter4*[] detected = DXGIHelper.EnumArray((uint index, ref IDXGIAdapter4* ptrOutput) =>
            {
                IDXGIAdapter1* ptr1 = null;
                int r = _dxgiFactory->EnumAdapters1(index, ref ptr1);
                ptrOutput = (IDXGIAdapter4*)ptr1;

                return r;
            });

            for (int i = 0; i < detected.Length; i++)
            {
                GraphicsCapabilities cap = _capabilitiesCallback(detected[i]);
                DisplayAdapterDXGI adapter = new DisplayAdapterDXGI(this, cap, detected[i]);
                _adapters.Add(adapter);

                if (adapter.Outputs.Count > 0)
                {
                    _withOutputs.Add(adapter);

                    // Set default if needed
                    if (_defaultAdapter == null)
                    {
                        _defaultAdapter = adapter;
                        _selectedAdapter = adapter;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void GetCompatibleAdapters(GraphicsCapabilities cap, List<IDisplayAdapter> adapters)
        {
            for (int i = 0; i < _adapters.Count; i++)
            {
                if (_adapters[i].Capabilities.IsCompatible(cap))
                    adapters.Add(_adapters[i]);
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<IDisplayAdapter> Adapters { get; }

        /// <inheritdoc/>
        public IReadOnlyList<IDisplayAdapter> AdaptersWithOutputs { get; }

        /// <inheritdoc/>
        public IDisplayAdapter DefaultAdapter => _defaultAdapter;

        /// <inheritdoc/>
        public IDisplayAdapter SelectedAdapter
        {
            get => _selectedAdapter;
            set
            {
                if (value != null)
                {
                    if (value is not DisplayAdapterDXGI dxgiAdapter)
                        throw new AdapterException(value, "The adapter is not a valid DXGI adapter.");

                    if (value.Manager != this)
                        throw new AdapterException(value, "The adapter not owned by the current display manager.");

                    _selectedAdapter = dxgiAdapter;
                }
                else
                {
                    _selectedAdapter = null;
                }
            }
        }

        /// <inheritdoc/>
        public IDisplayAdapter this[DeviceID id]
        {
            get
            {
                foreach(IDisplayAdapter adapter in _adapters)
                {
                    if (adapter.ID == id)
                        return adapter;
                }

                return null;
            }
        }

        /// <summary>Gets the DXGI factory.</summary>
        public IDXGIFactory7* DxgiFactory => _dxgiFactory;

        /// <summary>
        /// Gets the <see cref="Logger"/> attached to the current <see cref="DisplayManagerDXGI"/> instance.
        /// </summary>
        internal Logger Log { get; private set; }
    }
}
