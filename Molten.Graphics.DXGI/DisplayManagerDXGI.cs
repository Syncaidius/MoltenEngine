using Molten.Windows32;
using Silk.NET.Core.Native;
using Silk.NET.DXGI;

namespace Molten.Graphics.Dxgi
{
    public unsafe delegate void DXGIDetectCapabilitiesCallback(GraphicsSettings settings, DisplayAdapterDXGI adapter);

    public unsafe class DisplayManagerDXGI : GraphicsDisplayManager
    {
        const uint DXGI_CREATE_FACTORY_NODEBUG = 0x0;
        const uint DXGI_CREATE_FACTORY_DEBUG = 0x01;

        DXGI _api;
        IDXGIFactory7* _dxgiFactory;
        List<DisplayAdapterDXGI> _adapters;

        DisplayAdapterDXGI _defaultAdapter;
        DisplayAdapterDXGI _selectedAdapter;
        DXGIDetectCapabilitiesCallback _capabilitiesCallback;

        public DisplayManagerDXGI(DXGIDetectCapabilitiesCallback capabilitiesCallback)
        {
            _api = DXGI.GetApi();
            _capabilitiesCallback = capabilitiesCallback; 
            _adapters = new List<DisplayAdapterDXGI>();
            Adapters = _adapters.AsReadOnly();
        }

        protected override void OnDispose()
        {
            SilkUtil.ReleasePtr(ref _dxgiFactory);
            _api.Dispose();
        }

        /// <inheritdoc/>
        protected override void OnInitialize(Logger log, GraphicsSettings settings)
        {
            // Create factory
            Guid factoryGuid = IDXGIFactory2.Guid;
            void* ptrFactory = null;
            uint debugFlag = settings.EnableDebugLayer ? DXGI_CREATE_FACTORY_DEBUG : DXGI_CREATE_FACTORY_NODEBUG;

            int r = _api.CreateDXGIFactory2(debugFlag, &factoryGuid, &ptrFactory);
            DxgiError err = DXGIHelper.ErrorFromResult(r);
            if (err != DxgiError.Ok)
            {
                log.Error($"Failed to initialize DXGI: {err}");
                return;
            }

            IDXGIFactory2* tmp = (IDXGIFactory2*)ptrFactory;
            Guid factory7Guid = IDXGIFactory7.Guid;

            r = tmp->QueryInterface(&factory7Guid, &ptrFactory);
            err = DXGIHelper.ErrorFromResult(r);
            if (err != DxgiError.Ok)
            {
                log.Error($"Failed to query DXGI 1.6 factory: {err}");
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
                DisplayAdapterDXGI adapter = new DisplayAdapterDXGI(this, detected[i]);
                _capabilitiesCallback(settings, adapter);
                _adapters.Add(adapter);

                if (adapter.Outputs.Count > 0)
                {
                    // Set default if needed
                    if (_defaultAdapter == null && adapter.Capabilities.Api != GraphicsApi.Unsupported)
                    {
                        _defaultAdapter = adapter;
                        _selectedAdapter = adapter;
                    }
                }
            }

            // If no adapter with outputs was found, use the first detected adapter.
            if (_defaultAdapter == null && _adapters.Count > 0)
            {
                for(int i = 0; i < _adapters.Count; i++)
                {
                    if (_adapters[i].Capabilities.Api != GraphicsApi.Unsupported)
                    {
                        _defaultAdapter = _adapters[i];
                        _selectedAdapter = _defaultAdapter;
                        break;
                    }
                }
            }

            // Do we still not have an adapter? Unsupported.
            if(_defaultAdapter == null)
                Log.Error($"No supported GPU adapter found.");
        }

        public IDXGISwapChain4* CreateSwapChain(DisplayModeDXGI mode, GraphicsSettings settings, Logger log, IUnknown* ptrDevice, IntPtr windowHandle)
        {
            SwapChainDesc1 desc = new SwapChainDesc1()
            {
                Width = mode.Width,
                Height = mode.Height,
                Format = mode.Format.ToApi(),
                BufferUsage = (uint)DxgiUsage.RenderTargetOutput,
                BufferCount = settings.GetBackBufferSize(),
                SampleDesc = new SampleDesc(1, 0), // TODO support multi-sampling: https://learn.microsoft.com/en-us/windows/win32/api/dxgicommon/ns-dxgicommon-dxgi_sample_desc
                SwapEffect = SwapEffect.Discard,
                Flags = (uint)DxgiSwapChainFlags.None,
                Stereo = 0,
                Scaling = Scaling.Stretch,
                AlphaMode = AlphaMode.Ignore // TODO implement this correctly
            };

            IDXGISwapChain1* ptrSwap1 = null;
            WinHResult hr = DxgiFactory->CreateSwapChainForHwnd(ptrDevice, windowHandle, ref desc, null, null, ref ptrSwap1);
            DxgiError de = hr.ToEnum<DxgiError>();

            if (de != DxgiError.Ok)
            {
                log.Error($"Creation of swapchain failed with result: {de}");
                return null;
            }

            Guid swap4Guid = IDXGISwapChain4.Guid;
            void* nativeSwap = null;
            int r = ptrSwap1->QueryInterface(&swap4Guid, &nativeSwap);
            return (IDXGISwapChain4*)nativeSwap;
        }

        /// <inheritdoc/>
        public override void GetCompatibleAdapters(GraphicsCapabilities cap, List<IDisplayAdapter> adapters)
        {
            for (int i = 0; i < _adapters.Count; i++)
            {
                if (_adapters[i].Capabilities.IsCompatible(cap))
                    adapters.Add(_adapters[i]);
            }
        }

        /// <inheritdoc/>
        public override IReadOnlyList<IDisplayAdapter> Adapters { get; }

        /// <inheritdoc/>
        public override IDisplayAdapter DefaultAdapter => _defaultAdapter;

        /// <inheritdoc/>
        public override IDisplayAdapter SelectedAdapter
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

        /// <summary>Gets the DXGI factory.</summary>
        public IDXGIFactory7* DxgiFactory => _dxgiFactory;
    }
}
