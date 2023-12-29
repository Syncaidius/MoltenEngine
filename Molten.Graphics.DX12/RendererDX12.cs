using Molten.Graphics.Dxc;
using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX12
{
    public unsafe class RendererDX12 : RenderService
    {
        /// <summary>
        /// The maximum allowed root signature version in this renderer.
        /// </summary>
        internal const D3DRootSignatureVersion MAX_ROOT_SIG_VERSION = D3DRootSignatureVersion.Version11;

        D3D12 _api;
        GraphicsManagerDXGI _displayManager;
        ID3D12Debug6* _debug;

        public RendererDX12() { }

        protected override unsafe GraphicsManager OnInitializeDisplayManager(GraphicsSettings settings)
        {
            _api = D3D12.GetApi();

            // The DX12 debug layer must be setup before any devices are created.
            if (settings.EnableDebugLayer)
            {
                Guid guidDebug = ID3D12Debug6.Guid;
                void* ptr = null;
                _api.GetDebugInterface(&guidDebug, &ptr);
                _debug = (ID3D12Debug6*)ptr;
                _debug->EnableDebugLayer();
            }

            Builder = new DeviceBuilderDX12(_api, this);
            _displayManager = new GraphicsManagerDXGI(CreateDevice, Builder.GetCapabilities);
            return _displayManager;
        }

        private unsafe DeviceDXGI CreateDevice(GraphicsSettings settings, GraphicsManagerDXGI manager, IDXGIAdapter4* adapter)
        {
            return new DeviceDX12(this, _displayManager, adapter, Builder);
        }

        protected override List<GraphicsDevice> OnInitializeDevices(GraphicsSettings settings, GraphicsManager manager)
        {
            List<GraphicsDevice> result = new List<GraphicsDevice>();
            
            NativeDevice = _displayManager.PrimaryDevice as DeviceDX12;
            NativeDevice.Initialize();
            result.Add(NativeDevice);

            return result;
        }

        protected override void OnInitializeRenderer(EngineSettings settings)
        {
            
        }

        protected override void OnDisposeBeforeRender()
        {
            NativeDevice?.Dispose();

            SilkUtil.ReleasePtr(ref _debug);
            _api.Dispose();
        }

        internal DeviceDX12 NativeDevice { get; private set; }

        internal DeviceBuilderDX12 Builder { get; private set; }

        protected override DxcCompiler Compiler { get; }

        internal D3D12 Api => _api;

        internal ID3D12Debug6* Debug => _debug;
    }
}
